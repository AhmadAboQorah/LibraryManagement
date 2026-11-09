const msg = document.getElementById("msg");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");
const tabLogin = document.getElementById("tabLogin");
const tabRegister = document.getElementById("tabRegister");

renderNav("login");
if (Session.isLoggedIn()) location.href = "books.html";
if (new URLSearchParams(location.search).get("expired")) {
  showMessage(msg, "Your session expired. Please log in again.", "error");
}

function switchTab(toRegister) {
  loginForm.hidden = toRegister;
  registerForm.hidden = !toRegister;
  tabLogin.classList.toggle("active", !toRegister);
  tabRegister.classList.toggle("active", toRegister);
  showMessage(msg, "");
}
tabLogin.addEventListener("click", () => switchTab(false));
tabRegister.addEventListener("click", () => switchTab(true));

async function login(email, password) {
  const data = await api("/api/auth/login", {
    method: "POST", body: { email, password }, protectedCall: false,
  });
  Session.save(data.token, data.user);   // token goes to localStorage
  location.href = "books.html";
}

loginForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  try {
    await login(document.getElementById("lEmail").value, document.getElementById("lPassword").value);
  } catch (err) {
    showMessage(msg, err.message);
  }
});

registerForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const email = document.getElementById("rEmail").value;
  const password = document.getElementById("rPassword").value;
  try {
    await api("/api/auth/register", {
      method: "POST", protectedCall: false,
      body: { fullName: document.getElementById("rName").value, email, password },
    });
    await login(email, password);   // log the new member in straight away
  } catch (err) {
    showMessage(msg, err.message);
  }
});
