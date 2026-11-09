const msg = document.getElementById("msg");

if (requireLogin("Admin")) {
  renderNav("admin");
  load();
}

async function load() {
  try {
    const [authors, categories] = await Promise.all([api("/api/authors"), api("/api/categories")]);
    renderList("authors", authors, "authors", a => a.name + (a.bio ? " — " + a.bio : ""));
    renderList("categories", categories, "categories", c => c.name);
  } catch (err) { showMessage(msg, err.message); }
}

function renderList(boxId, items, route, label) {
  const box = document.getElementById(boxId);
  box.replaceChildren();
  if (items.length === 0) { box.append(el("div", { class: "empty" }, "Nothing here yet.")); return; }
  const body = el("tbody");
  for (const item of items) {
    body.append(el("tr", {},
      el("td", {}, label(item)),
      el("td", {}, el("button", { class: "small danger", onclick: () => remove(route, item.id) }, "Delete"))));
  }
  box.append(el("table", {}, body));
}

async function remove(route, id) {
  if (!confirm("Delete this item?")) return;
  try {
    await api(`/api/${route}/${id}`, { method: "DELETE" });
    showMessage(msg, "Deleted.", "success");
    load();
  } catch (err) { showMessage(msg, err.message); }   // e.g. "This category contains books..."
}

document.getElementById("authorForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  try {
    await api("/api/authors", { method: "POST",
      body: { name: document.getElementById("aName").value, bio: document.getElementById("aBio").value || null } });
    e.target.reset();
    showMessage(msg, "Author added.", "success");
    load();
  } catch (err) { showMessage(msg, err.message); }
});

document.getElementById("categoryForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  try {
    await api("/api/categories", { method: "POST", body: { name: document.getElementById("cName").value } });
    e.target.reset();
    showMessage(msg, "Category added.", "success");
    load();
  } catch (err) { showMessage(msg, err.message); }
});
