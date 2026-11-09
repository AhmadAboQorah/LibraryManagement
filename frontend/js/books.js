const msg = document.getElementById("msg");
const listBox = document.getElementById("list");
const searchInput = document.getElementById("search");
const categoryFilter = document.getElementById("categoryFilter");
const addBtn = document.getElementById("addBtn");

let categories = [];
let authors = [];

renderNav("books");

// ---------- load data ----------
async function loadLookups() {
  try {
    [categories, authors] = await Promise.all([api("/api/categories"), api("/api/authors")]);
    for (const c of categories) categoryFilter.append(el("option", { value: c.id }, c.name));
  } catch (err) { showMessage(msg, err.message); }
}

async function loadBooks() {
  const params = new URLSearchParams();
  if (searchInput.value.trim()) params.set("title", searchInput.value.trim());
  if (categoryFilter.value) params.set("categoryId", categoryFilter.value);
  const qs = params.toString();

  try {
    const books = await api("/api/books" + (qs ? "?" + qs : ""));
    renderBooks(books);
  } catch (err) { showMessage(msg, err.message); }
}

// ---------- render ----------
function renderBooks(books) {
  listBox.replaceChildren();
  if (books.length === 0) {
    listBox.append(el("div", { class: "empty" }, "No books found."));
    return;
  }

  const head = el("tr", {}, ...["Title", "Authors", "Category", "Year", "Available", ""].map(h => el("th", {}, h)));
  const table = el("table", {}, el("thead", {}, head));
  const body = el("tbody");

  for (const b of books) {
    const actions = el("div", { class: "actions" });
    if (Session.isUser()) {
      actions.append(el("button", {
        class: "small", disabled: b.availableCopies === 0,
        onclick: () => borrow(b),
      }, b.availableCopies === 0 ? "Unavailable" : "Borrow"));
    }
    if (Session.isAdmin()) {     // buttons the user is not allowed to use are simply not created
      actions.append(
        el("button", { class: "small secondary", onclick: () => openForm(b) }, "Edit"),
        el("button", { class: "small danger", onclick: () => removeBook(b) }, "Delete"));
    }

    body.append(el("tr", {},
      el("td", {}, el("strong", {}, b.title), el("div", { class: "muted" }, "ISBN " + b.isbn)),
      el("td", {}, b.authors.map(a => a.name).join(", ")),
      el("td", {}, b.categoryName),
      el("td", {}, b.publishYear),
      el("td", {}, el("span", { class: "badge " + (b.availableCopies > 0 ? "ok" : "bad") },
        `${b.availableCopies} / ${b.totalCopies}`)),
      el("td", {}, actions)));
  }
  table.append(body);
  listBox.append(table);
}

// ---------- actions ----------
async function borrow(book) {
  try {
    const loan = await api("/api/loans", { method: "POST", body: { bookId: book.id } });
    showMessage(msg, `You borrowed "${book.title}". Return it before ${fmtDate(loan.dueDate)}.`, "success");
    loadBooks();
  } catch (err) {
    showMessage(msg, err.message);      // e.g. "You cannot borrow more than 3 books..."
  }
}

async function removeBook(book) {
  if (!confirm(`Delete "${book.title}"?`)) return;
  try {
    await api("/api/books/" + book.id, { method: "DELETE" });
    showMessage(msg, "Book deleted.", "success");
    loadBooks();
  } catch (err) { showMessage(msg, err.message); }
}

// ---------- add / edit form (Admin only) ----------
function openForm(book) {
  const editing = !!book;
  const formMsg = el("div");
  const f = {
    title: el("input", { type: "text", required: true, value: book?.title ?? "" }),
    isbn: el("input", { type: "text", required: true, minlength: 10, maxlength: 20, value: book?.isbn ?? "" }),
    year: el("input", { type: "number", required: true, min: 1000, max: 2100, value: book?.publishYear ?? new Date().getFullYear() }),
    copies: el("input", { type: "number", required: true, min: 1, value: book?.totalCopies ?? 1 }),
    category: el("select", { required: true },
      el("option", { value: "" }, "Select category"),
      categories.map(c => el("option", { value: c.id, selected: book?.categoryId === c.id }, c.name))),
  };
  const authorChecks = el("div", { class: "checks" }, authors.map(a =>
    el("label", {}, el("input", {
      type: "checkbox", value: a.id, checked: !!book?.authors.some(x => x.id === a.id),
    }), a.name)));

  const close = () => bg.remove();
  const form = el("form", {},
    el("h2", {}, editing ? "Edit book" : "Add book"),
    formMsg,
    el("label", {}, "Title"), f.title,
    el("label", {}, "ISBN"), f.isbn,
    el("label", {}, "Publish year"), f.year,
    el("label", {}, "Total copies"), f.copies,
    el("label", {}, "Category"), f.category,
    el("label", {}, "Authors"), authorChecks,
    el("p", { class: "row" },
      el("button", { type: "submit" }, "Save"),
      el("button", { type: "button", class: "secondary", onclick: close }, "Cancel")));

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const authorIds = [...authorChecks.querySelectorAll("input:checked")].map(i => Number(i.value));
    if (authorIds.length === 0) { showMessage(formMsg, "Select at least one author."); return; }

    const payload = {
      title: f.title.value, isbn: f.isbn.value, publishYear: Number(f.year.value),
      totalCopies: Number(f.copies.value), categoryId: Number(f.category.value), authorIds,
    };
    try {
      await api(editing ? "/api/books/" + book.id : "/api/books",
        { method: editing ? "PUT" : "POST", body: payload });
      close();
      showMessage(msg, editing ? "Book updated." : "Book added.", "success");
      loadBooks();
    } catch (err) { showMessage(formMsg, err.message); }
  });

  const bg = el("div", { class: "modal-bg" }, el("div", { class: "modal" }, form));
  document.body.append(bg);
}

// ---------- wire up ----------
document.getElementById("searchBtn").addEventListener("click", loadBooks);
searchInput.addEventListener("keydown", (e) => { if (e.key === "Enter") loadBooks(); });
categoryFilter.addEventListener("change", loadBooks);
document.getElementById("resetBtn").addEventListener("click", () => {
  searchInput.value = ""; categoryFilter.value = ""; loadBooks();
});

if (Session.isAdmin()) {
  addBtn.hidden = false;
  addBtn.addEventListener("click", () => openForm(null));
}

(async () => { await loadLookups(); await loadBooks(); })();
