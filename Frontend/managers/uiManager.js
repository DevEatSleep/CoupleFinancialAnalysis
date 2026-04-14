// uiManager.js - Handles UI updates and styling

const UIManager = {
    updateConnectionStatus(isConnected) {
        const status = document.getElementById("connection-status");
        if (isConnected) {
            status.textContent = "● Online";
            status.classList.add("online");
            status.classList.remove("offline");
        } else {
            status.textContent = "● Offline";
            status.classList.remove("online");
            status.classList.add("offline");
        }
    },

    updateButtonLabels() {
        const firstNames = SyncManager.getFirstNames();
        const womanName = firstNames["woman"] || "Woman";
        const manName = firstNames["man"] || "Man";
        
        const womanBtn = document.getElementById("woman-btn");
        const manBtn = document.getElementById("man-btn");
        
        if (womanBtn) {
            // Use translated template with name substitution
            const womanLabel = t("buttons.womanQuestions");
            if (womanLabel && womanLabel !== "buttons.womanQuestions") {
                const replaced = womanLabel.replace("Woman", womanName);
                womanBtn.textContent = replaced;
            }
        }
        if (manBtn) {
            // Use translated template with name substitution
            const manLabel = t("buttons.manQuestions");
            if (manLabel && manLabel !== "buttons.manQuestions") {
                const replaced = manLabel.replace("Man", manName);
                manBtn.textContent = replaced;
            }
        }
    },

    getAvatarForUser(sender) {
        return sender === "Person1" ? "👨" : "👩";
    },

    setupButtonHandlers() {
        const womanBtn = document.getElementById("woman-btn");
        const manBtn = document.getElementById("man-btn");
        const expensesBtn = document.getElementById("expenses-btn");
        
        if (womanBtn) {
            womanBtn.onclick = () => BotManager.askWomanQuestions();
        }
        
        if (manBtn) {
            manBtn.onclick = () => BotManager.askManQuestions();
        }
        
        if (expensesBtn) {
            expensesBtn.onclick = () => BotManager.startExpenseMode();
        }
    },

    setupInputHandler() {
        const input = document.getElementById("message-input");
        if (input) {
            input.addEventListener("keypress", (e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    ChatInputHandler.sendMessage();
                }
            });
        }
    }
};
