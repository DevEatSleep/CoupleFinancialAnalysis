// chatInputHandler.js - Handles user message input

const ChatInputHandler = {
    sendMessage() {
        const input = document.getElementById("message-input");
        const content = input.value.trim();

        // Always prevent empty messages - bot will not advance without a response
        if (!content) {
            return;
        }

        const contentLower = content.toLowerCase();
        
        // Get translated commands (support both English and current language)
        const yesCmd = t("commands.yes").toLowerCase();
        const doneCmd = t("commands.done").toLowerCase();
        const isYes = contentLower === "yes" || contentLower === yesCmd;
        const isDone = contentLower === "done" || contentLower === doneCmd;

        // Only allow messages when waiting for bot response or in expense mode
        if (!ChatState.isBotWaiting && !ExpenseManager.expenseMode) {
            input.value = "";
            return;
        }

        // Handle expense mode commands
        if (ExpenseManager.expenseMode && !ChatState.isBotWaiting) {
            if (isDone) {
                MessageManager.addChatMessage(ChatState.currentUser, content, UIManager.getAvatarForUser(ChatState.currentUser), new Date().toISOString());
                MessageManager.addChatMessage("🤖 Bot", t("bot.expenseFinished"), "🤖", new Date().toISOString());
                ExpenseManager.expenseMode = false;
                ExpenseManager.currentExpense = {};
                document.getElementById("message-input").placeholder = "Type your message...";
                input.value = "";
                return;
            } else if (isYes) {
                MessageManager.addChatMessage(ChatState.currentUser, content, UIManager.getAvatarForUser(ChatState.currentUser), new Date().toISOString());
                ExpenseManager.currentExpense = {};
                // Restart expense questions
                setTimeout(() => {
                    fetch("http://localhost:5000/api/bot/next-question/shared")
                        .then(res => {
                            if (!res.ok) throw new Error(`HTTP ${res.status}`);
                            return res.json();
                        })
                        .then(data => {
                            ChatState.currentBotQuestion = data;
                            ChatState.isBotWaiting = true;
                            MessageManager.addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
                            document.getElementById("message-input").placeholder = "Respond to bot...";
                        })
                        .catch(err => {
                            console.error("Failed to get next expense question:", err);
                        });
                }, 300);
                input.value = "";
                return;
            }
        }

        // If waiting for bot response, handle it through respondToBot
        if (ChatState.isBotWaiting && ChatState.currentBotQuestion) {
            BotManager.respondToBot(content);
            input.value = "";
            return;
        }

        input.value = "";
    }
};
