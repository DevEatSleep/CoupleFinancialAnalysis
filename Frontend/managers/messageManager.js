// messageManager.js - Handles chat messages and polling

let lastMessageCount = 0;
let pollInterval;

const MessageManager = {
    startPolling() {
        console.log("startPolling called");
        UIManager.updateConnectionStatus(true);
        
        // Load initial messages
        this.loadMessagesFromServer();
        
        // Give DOM time to fully render, then load initial data
        setTimeout(() => {
            DashboardManager.loadResponses();
            ExpenseManager.loadExpenses();
            UIManager.updateButtonLabels();
        }, 100);
        
        // Poll for updates
        pollInterval = setInterval(() => {
            this.checkForNewMessages();
            DashboardManager.loadResponses();
            ExpenseManager.loadExpenses();
            UIManager.updateButtonLabels();
        }, 2000);
        
        console.log("Polling started");
    },

    checkForNewMessages() {
        fetch("http://localhost:5000/api/chat")
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(messages => {
                // Only add new messages
                if (messages.length > lastMessageCount) {
                    const newMessages = messages.slice(lastMessageCount);
                    newMessages.forEach(msg => {
                        // Don't re-add if already in UI
                        if (!this.isMessageInUI(msg)) {
                            this.addChatMessage(msg.sender, msg.content, msg.avatarUrl, msg.createdAt);
                        }
                    });
                    lastMessageCount = messages.length;
                }
                UIManager.updateConnectionStatus(true);
            })
            .catch(err => {
                console.error("Poll error:", err);
                UIManager.updateConnectionStatus(false);
            });
    },

    isMessageInUI(msg) {
        const chatMessages = document.getElementById("chat-messages");
        return Array.from(chatMessages.querySelectorAll(".chat-message")).some(el => 
            el.textContent.includes(msg.content)
        );
    },

    addChatMessage(sender, content, avatarUrl, createdAt, isOptimistic = false) {
        const chatMessages = document.getElementById("chat-messages");
        
        if (chatMessages.querySelector(".empty-state")) {
            chatMessages.innerHTML = "";
        }

        const msgDiv = document.createElement("div");
        msgDiv.className = `chat-message ${sender === ChatState.currentUser ? "mine" : ""}`;
        
        // Get display name (use first name if available, otherwise use sender)
        const displayName = this.getDisplayName(sender);
        
        msgDiv.innerHTML = `
            <div class="sender">${displayName}</div>
            <div>${this.escapeHtml(content)}</div>
            ${isOptimistic ? '<div style="opacity:0.7;font-size:12px;">⏳ Sending...</div>' : ''}
        `;

        chatMessages.appendChild(msgDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    },

    getDisplayName(sender) {
        // Handle bot messages
        if (sender === "🤖 Bot") return "🤖 Bot";
        
        // Map person identifiers to their first names
        const firstNames = SyncManager.getFirstNames();
        if (sender === "Person1") return firstNames["woman"] || "Woman";
        if (sender === "Person2") return firstNames["man"] || "Man";
        
        // Fallback to sender as-is
        return sender;
    },

    loadMessagesFromServer() {
        fetch("http://localhost:5000/api/chat")
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(messages => {
                document.getElementById("chat-messages").innerHTML = "";

                messages.forEach(msg => {
                    this.addChatMessage(msg.sender, msg.content, msg.avatarUrl, msg.createdAt);
                });

                lastMessageCount = messages.length;
            })
            .catch(err => {
                console.error("Failed to load messages:", err);
            });
    },

    escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    },

    clearChat() {
        const chatMessages = document.getElementById("chat-messages");
        chatMessages.innerHTML = "";
    }
};
