window.adminModal = {
  open(id) {
    document.getElementById(id)?.classList.add("is-open");
  },
  close(id) {
    document.getElementById(id)?.classList.remove("is-open");
  }
};
