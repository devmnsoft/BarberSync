window.adminToast = {
  show(message, type = "info") {
    const host = document.querySelector("[data-admin-toast-host]") || document.body;
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    host.appendChild(toast);
    setTimeout(() => toast.remove(), 3200);
  }
};
