document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
    button.addEventListener("click", () => document.body.classList.toggle("sidebar-collapsed"));
  });
});
