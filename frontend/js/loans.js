const msg = document.getElementById("msg");
const listBox = document.getElementById("list");

if (requireLogin()) {
  renderNav("loans");
  const admin = Session.isAdmin();
  document.getElementById("title").textContent = admin ? "Overdue loans" : "My loans";
  loadLoans();
}

async function loadLoans() {
  try {
    // User: GET /api/loans/my      Admin: GET /api/loans/overdue
    const loans = await api(Session.isAdmin() ? "/api/loans/overdue" : "/api/loans/my");
    renderLoans(loans);
  } catch (err) { showMessage(msg, err.message); }
}

function statusBadge(l) {
  const cls = l.status === "Returned" ? "gray" : l.status === "Overdue" ? "bad" : "ok";
  return el("span", { class: "badge " + cls }, l.status);
}

function renderLoans(loans) {
  listBox.replaceChildren();
  if (loans.length === 0) {
    listBox.append(el("div", { class: "empty" }, Session.isAdmin() ? "No overdue loans 🎉" : "You have no loans yet."));
    return;
  }

  const admin = Session.isAdmin();
  const headers = ["Book", ...(admin ? ["Member"] : []), "Borrowed", "Due", "Returned", "Status", ...(admin ? [] : [""])];
  const table = el("table", {}, el("thead", {}, el("tr", {}, headers.map(h => el("th", {}, h)))));
  const body = el("tbody");

  for (const l of loans) {
    const row = el("tr", {},
      el("td", {}, l.bookTitle),
      admin ? el("td", {}, l.userName) : null,
      el("td", {}, fmtDate(l.loanDate)),
      el("td", {}, fmtDate(l.dueDate)),
      el("td", {}, fmtDate(l.returnDate)),
      el("td", {}, statusBadge(l)));
    if (!admin) {
      row.append(el("td", {}, l.returnDate ? "" :
        el("button", { class: "small", onclick: () => returnBook(l) }, "Return")));
    }
    body.append(row);
  }
  table.append(body);
  listBox.append(table);
}

async function returnBook(loan) {
  try {
    await api(`/api/loans/${loan.id}/return`, { method: "PUT" });
    showMessage(msg, `"${loan.bookTitle}" returned. Thank you!`, "success");
    loadLoans();
  } catch (err) { showMessage(msg, err.message); }
}
