// chat.js - Main initialization and controller

document.addEventListener("DOMContentLoaded", () => {
    // Initialize global state
    ChatState.currentUser = SyncManager.getSender();

    // Setup UI event handlers
    UIManager.setupInputHandler();
    UIManager.setupButtonHandlers();
    UIManager.updateButtonLabels();

    // Start polling for updates
    MessageManager.startPolling();
});
