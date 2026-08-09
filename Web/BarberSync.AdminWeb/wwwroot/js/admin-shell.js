document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
    button.addEventListener("click", () => document.querySelector('.admin-sidebar')?.classList.toggle("is-open"));
  });
});
