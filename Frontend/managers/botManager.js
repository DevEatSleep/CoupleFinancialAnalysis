// botManager.js - Handles bot questions and user responses

const BotManager = {
    startExpenseMode() {
        ExpenseManager.expenseMode = true;
        ChatState.currentPersonType = null;
        ExpenseManager.currentExpense = {};
        
        // Clear chat and show fresh start
        MessageManager.clearChat();
        
        // Show help message
        MessageManager.addChatMessage("🤖 Bot", t("bot.welcome"), "🤖", new Date().toISOString());
        
        // Start asking for the first expense
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
                    document.getElementById("message-input").placeholder = "Enter expense label...";
                })
                .catch(err => {
                    console.error("Failed to get expense question:", err);
                    MessageManager.addChatMessage("🤖 Bot", t("bot.error"), "🤖", new Date().toISOString());
                });
        }, 500);
    },

    askWomanQuestions() {
        // Clear chat
        MessageManager.clearChat();
        
        ChatState.currentPersonType = "woman";
        this.askNextQuestion();
    },

    askManQuestions() {
        // Clear chat
        MessageManager.clearChat();
        
        ChatState.currentPersonType = "man";
        this.askNextQuestion();
    },

    askNextQuestion() {
        const url = ChatState.currentPersonType 
            ? `http://localhost:5000/api/bot/next-question/${ChatState.currentPersonType}`
            : "http://localhost:5000/api/bot/next-question";
        
        fetch(url)
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
                console.error("Failed to get question:", err);
                const firstNames = SyncManager.getFirstNames();
                const womanName = firstNames["woman"] || t("person.woman");
                const manName = firstNames["man"] || t("person.man");
                
                if (ChatState.currentPersonType === "woman") {
                    const msg = t("bot.allQuestionsAnswered").replace("{person}", womanName).replace("{otherPerson}", "Man");
                    MessageManager.addChatMessage("🤖 Bot", msg, "🤖", new Date().toISOString());
                } else if (ChatState.currentPersonType === "man") {
                    const msg = t("bot.allDone").replace("{person}", manName);
                    MessageManager.addChatMessage("🤖 Bot", msg, "🤖", new Date().toISOString());
                }
                ChatState.currentPersonType = null;
                ChatState.isBotWaiting = false;
            });
    },

    respondToBot(response) {
        if (!ChatState.currentBotQuestion) return;

        // Check if this is a first name question (questionId 1 for woman, 4 for man) and extract the name
        const qId = ChatState.currentBotQuestion.id || ChatState.currentBotQuestion.questionId;
        if (qId === 1 || qId === 4) {
            SyncManager.saveFirstName(ChatState.currentPersonType, response);
            console.log(`Saved first name for ${ChatState.currentPersonType}: ${response}`);
        }

        // Handle expense questions
        if (ChatState.currentBotQuestion.category === "expenses") {
            const qId = ChatState.currentBotQuestion.id || ChatState.currentBotQuestion.questionId;
            
            // Question IDs: 7 = label, 8 = amount, 9 = paidBy
            if (qId === 7) {
                ExpenseManager.currentExpense.label = response;
            } else if (qId === 8) {
                ExpenseManager.currentExpense.amount = response;
            } else if (qId === 9) {
                ExpenseManager.currentExpense.paidBy = response;
                
                // Save the complete expense
                if (ExpenseManager.currentExpense.label && ExpenseManager.currentExpense.amount && ExpenseManager.currentExpense.paidBy) {
                    ExpenseManager.saveExpense(ExpenseManager.currentExpense.label, ExpenseManager.currentExpense.amount, ExpenseManager.currentExpense.paidBy);
                    const expenseMsg = t("bot.expenseRecorded")
                        .replace("{label}", ExpenseManager.currentExpense.label)
                        .replace("{amount}", ExpenseManager.currentExpense.amount)
                        .replace("{paidBy}", ExpenseManager.currentExpense.paidBy);
                    MessageManager.addChatMessage("🤖 Bot", expenseMsg, "🤖", new Date().toISOString());
                    ExpenseManager.currentExpense = {}; // Reset for next expense
                    ChatState.isBotWaiting = false;
                    ChatState.currentBotQuestion = null;
                    document.getElementById("message-input").placeholder = "Type your message...";
                    return;
                }
            }
            
            // Add user response to chat
            MessageManager.addChatMessage(ChatState.currentUser, response, UIManager.getAvatarForUser(ChatState.currentUser), new Date().toISOString());
            
            // Ask next expense question
            ChatState.isBotWaiting = false;
            ChatState.currentBotQuestion = null;
            document.getElementById("message-input").placeholder = "Type your message...";
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
            }, 500);
            return;
        }

        fetch("http://localhost:5000/api/bot/respond", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                questionId: ChatState.currentBotQuestion.id,
                userResponse: response
            })
        })
            .then(res => {
                if (!res.ok) {
                    throw new Error(`HTTP ${res.status}`);
                }
                return res.json();
            })
            .then(data => {
                // Add user response to chat
                MessageManager.addChatMessage(ChatState.currentUser, response, UIManager.getAvatarForUser(ChatState.currentUser), new Date().toISOString());
                
                // Refresh dashboard table with new response
                DashboardManager.loadResponses();
                
                // Reset bot state
                ChatState.isBotWaiting = false;
                ChatState.currentBotQuestion = null;
                document.getElementById("message-input").placeholder = "Type your message...";
                
                // If in question mode, ask the next question
                if (ChatState.currentPersonType) {
                    setTimeout(() => this.askNextQuestion(), 500);
                }
            })
            .catch(err => {
                console.error("Failed to respond to bot:", err);
                ChatState.isBotWaiting = false;
                ChatState.currentBotQuestion = null;
                document.getElementById("message-input").placeholder = "Type your message...";
            });
    }
};
