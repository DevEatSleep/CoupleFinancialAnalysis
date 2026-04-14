// chat.js - Main initialization and controller
document.addEventListener("DOMContentLoaded", () => {
    ChatState.currentUser = SyncManager.getSender();
    UIManager.setupInputHandler();
    UIManager.setupButtonHandlers();
    UIManager.updateButtonLabels();
    MessageManager.startPolling();
});

