let currentUser = "Person1";
let currentBotQuestion = null;
let isBotWaiting = false;
let lastMessageCount = 0;
let pollInterval;
let currentPersonType = null; // 'woman' or 'man'
let allResponses = {}; // Store responses organized by question text
let allExpenses = []; // Store all expenses
let currentExpense = {}; // Track current expense being built
let expenseMode = false; // Track if we're in expense entry mode

// Poll for new messages every 2 seconds
function startPolling() {
    console.log("startPolling called");
    updateConnectionStatus(true);
    
    // Load initial messages
    loadMessagesFromServer();
    
    // Give DOM time to fully render, then load responses
    setTimeout(() => {
        loadResponses();
        loadExpenses();
        updateButtonLabels();
    }, 100);
    
    // Poll for updates
    pollInterval = setInterval(() => {
        checkForNewMessages();
        loadResponses();
        loadExpenses();
        updateButtonLabels();
    }, 2000);
    
    console.log("Polling started");
}

// Check if there are new messages on the server
function checkForNewMessages() {
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
                    if (!isMessageInUI(msg)) {
                        addChatMessage(msg.sender, msg.content, msg.avatarUrl, msg.createdAt);
                    }
                });
                lastMessageCount = messages.length;
            }
            updateConnectionStatus(true);
        })
        .catch(err => {
            console.error("Poll error:", err);
            updateConnectionStatus(false);
        });
}

function isMessageInUI(msg) {
    const chatMessages = document.getElementById("chat-messages");
    return Array.from(chatMessages.querySelectorAll(".chat-message")).some(el => 
        el.textContent.includes(msg.content)
    );
}

function sendMessage() {
    const input = document.getElementById("message-input");
    const content = input.value.trim().toLowerCase();

    if (!content) return;

    // Handle expense mode commands
    if (expenseMode && !isBotWaiting) {
        if (content === "done") {
            addChatMessage(currentUser, "done", getAvatarForUser(currentUser), new Date().toISOString());
            addChatMessage("🤖 Bot", t("bot.expenseFinished"), "🤖", new Date().toISOString());
            expenseMode = false;
            currentExpense = {};
            document.getElementById("message-input").placeholder = "Type your message...";
            input.value = "";
            return;
        } else if (content === "yes") {
            addChatMessage(currentUser, "yes", getAvatarForUser(currentUser), new Date().toISOString());
            currentExpense = {};
            // Restart expense questions
            setTimeout(() => {
                fetch("http://localhost:5000/api/bot/next-question/shared")
                    .then(res => {
                        if (!res.ok) throw new Error(`HTTP ${res.status}`);
                        return res.json();
                    })
                    .then(data => {
                        currentBotQuestion = data;
                        isBotWaiting = true;
                        addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
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

    // If waiting for bot response
    if (isBotWaiting && currentBotQuestion) {
        respondToBot(content);
        input.value = "";
        return;
    }

    const sender = currentUser;
    const avatarUrl = getAvatarForUser(sender);

    // Add to UI immediately (optimistic)
    addChatMessage(sender, content, avatarUrl, new Date().toISOString(), true);

    // Clear input
    input.value = "";

    // Send to server
    fetch("http://localhost:5000/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            sender: sender,
            content: content,
            avatarUrl: avatarUrl
        })
    })
    .then(res => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
    })
    .then(data => {
        console.log("Message sent:", data);
    })
    .catch(err => {
        console.error("Failed to send message:", err);
    });
}

function addChatMessage(sender, content, avatarUrl, createdAt, isOptimistic = false) {
    const chatMessages = document.getElementById("chat-messages");
    
    if (chatMessages.querySelector(".empty-state")) {
        chatMessages.innerHTML = "";
    }

    const msgDiv = document.createElement("div");
    msgDiv.className = `chat-message ${sender === currentUser ? "mine" : ""}`;
    
    // Get display name (use first name if available, otherwise use sender)
    const displayName = getDisplayName(sender);
    
    msgDiv.innerHTML = `
        <div class="sender">${displayName}</div>
        <div>${escapeHtml(content)}</div>
        ${isOptimistic ? '<div style="opacity:0.7;font-size:12px;">⏳ Sending...</div>' : ''}
    `;

    chatMessages.appendChild(msgDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}



function getDisplayName(sender) {
    // Handle bot messages
    if (sender === "🤖 Bot") return "🤖 Bot";
    
    // Map person identifiers to their first names
    const firstNames = SyncManager.getFirstNames();
    if (sender === "Person1") return firstNames["woman"] || "Woman";
    if (sender === "Person2") return firstNames["man"] || "Man";
    
    // Fallback to sender as-is
    return sender;
}



function loadMessagesFromServer() {
    fetch("http://localhost:5000/api/chat")
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(messages => {
            document.getElementById("chat-messages").innerHTML = "";

            messages.forEach(msg => {
                addChatMessage(msg.sender, msg.content, msg.avatarUrl, msg.createdAt);
            });

            lastMessageCount = messages.length;
        })
        .catch(err => {
            console.error("Failed to load messages:", err);
        });
}

function updateConnectionStatus(isConnected) {
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
}

// Load responses from API and update dashboard table
function loadResponses() {
    fetch("http://localhost:5000/api/bot/responses")
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(responses => {
            console.log("Loaded responses:", responses);
            // Organize responses by person with name, age, salary
            allResponses = {
                woman: { name: "-", age: "-", salary: "-", nameId: null, ageId: null, salaryId: null },
                man: { name: "-", age: "-", salary: "-", nameId: null, ageId: null, salaryId: null }
            };
            
            responses.forEach(resp => {
                console.log("Processing response:", resp);
                const personValue = resp.person || resp.Person || "";
                const questionText = resp.questionText || resp.QuestionText || "";
                const userResponse = resp.userResponse || resp.UserResponse || "";
                const respId = resp.id || resp.Id;
                
                if (personValue === "woman" || personValue === "man") {
                    if (questionText.includes("first name")) {
                        allResponses[personValue].name = userResponse;
                        allResponses[personValue].nameId = respId;
                    } else if (questionText.includes("age")) {
                        allResponses[personValue].age = userResponse;
                        allResponses[personValue].ageId = respId;
                    } else if (questionText.includes("salary")) {
                        allResponses[personValue].salary = userResponse;
                        allResponses[personValue].salaryId = respId;
                    }
                }
            });
            console.log("Organized allResponses:", allResponses);
            updateDashboardTable();
        })
        .catch(err => {
            console.error("Failed to load responses:", err);
        });
}

function updateDashboardTable() {
    const tbody = document.getElementById("responses-tbody");
    console.log("tbody element:", tbody);
    if (!tbody) {
        console.error("responses-tbody element not found! Retrying...");
        // Retry after a short delay
        setTimeout(updateDashboardTable, 100);
        return;
    }

    if (!allResponses || (!allResponses.woman && !allResponses.man)) {
        tbody.innerHTML = '<tr><td colspan="5" class="empty-state">No responses yet</td></tr>';
        return;
    }

    tbody.innerHTML = "";
    const firstNames = SyncManager.getFirstNames();
    
    // Add woman row
    if (allResponses.woman) {
        const womanRow = document.createElement("tr");
        const womanName = firstNames["woman"] || allResponses.woman.name;
        womanRow.innerHTML = `
            <td><strong>👩 Woman</strong></td>
            <td class="editable" data-id="${allResponses.woman.nameId}" data-field="name">${escapeHtml(womanName)}</td>
            <td class="editable" data-id="${allResponses.woman.ageId}" data-field="age">${escapeHtml(allResponses.woman.age)}</td>
            <td class="editable" data-id="${allResponses.woman.salaryId}" data-field="salary">${escapeHtml(allResponses.woman.salary)}</td>
            <td><button class="delete-btn" onclick="deleteResponse(${allResponses.woman.nameId})">🗑️</button></td>
        `;
        womanRow.querySelectorAll(".editable").forEach(cell => {
            cell.addEventListener("click", handleCellClick);
        });
        tbody.appendChild(womanRow);
    }
    
    // Add man row
    if (allResponses.man) {
        const manRow = document.createElement("tr");
        const manName = firstNames["man"] || allResponses.man.name;
        manRow.innerHTML = `
            <td><strong>👨 Man</strong></td>
            <td class="editable" data-id="${allResponses.man.nameId}" data-field="name">${escapeHtml(manName)}</td>
            <td class="editable" data-id="${allResponses.man.ageId}" data-field="age">${escapeHtml(allResponses.man.age)}</td>
            <td class="editable" data-id="${allResponses.man.salaryId}" data-field="salary">${escapeHtml(allResponses.man.salary)}</td>
            <td><button class="delete-btn" onclick="deleteResponse(${allResponses.man.nameId})">🗑️</button></td>
        `;
        manRow.querySelectorAll(".editable").forEach(cell => {
            cell.addEventListener("click", handleCellClick);
        });
        tbody.appendChild(manRow);
    }
    
    // Update button labels with first names
    updateButtonLabels();
}

function updateButtonLabels() {
    const firstNames = SyncManager.getFirstNames();
    const womanName = firstNames["woman"] || "Woman";
    const manName = firstNames["man"] || "Man";
    
    const womanBtn = document.getElementById("woman-btn");
    const manBtn = document.getElementById("man-btn");
    
    if (womanBtn) {
        // Use translated template with name substitution
        const womanLabel = t("buttons.womanQuestions").replace("Woman", womanName);
        womanBtn.textContent = womanLabel;
    }
    if (manBtn) {
        // Use translated template with name substitution
        const manLabel = t("buttons.manQuestions").replace("Man", manName);
        manBtn.textContent = manLabel;
    }
}

function getAvatarForUser(sender) {
    return sender === "Person1" ? "👨" : "👩";
}

function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

// Load expenses from API and update dashboard
function loadExpenses() {
    fetch("http://localhost:5000/api/bot/expenses")
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(expenses => {
            console.log("Loaded expenses:", expenses);
            allExpenses = expenses;
            updateExpensesTable();
        })
        .catch(err => {
            console.error("Failed to load expenses:", err);
        });
}

function updateExpensesTable() {
    const tbody = document.getElementById("expenses-tbody");
    if (!tbody) return;

    if (!allExpenses || allExpenses.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="empty-state">No expenses yet</td></tr>';
        return;
    }

    tbody.innerHTML = "";
    allExpenses.forEach(expense => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td class="editable" data-expense-id="${expense.id}" data-field="label">${escapeHtml(expense.label)}</td>
            <td class="editable" data-expense-id="${expense.id}" data-field="amount">${expense.amount}</td>
            <td class="editable" data-expense-id="${expense.id}" data-field="paidBy">${escapeHtml(expense.paidBy)}</td>
            <td><button class="delete-btn" onclick="deleteExpense(${expense.id})">🗑️</button></td>
        `;
        row.querySelectorAll(".editable").forEach(cell => {
            cell.addEventListener("click", handleExpenseCellClick);
        });
        tbody.appendChild(row);
    });
}

function saveExpense(label, amount, paidBy) {
    fetch("http://localhost:5000/api/bot/expense", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            label: label,
            amount: parseFloat(amount),
            paidBy: paidBy
        })
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            console.log("Expense saved:", data);
            loadExpenses();
        })
        .catch(err => {
            console.error("Failed to save expense:", err);
        });
}

// Inline editing handlers
function handleCellClick(event) {
    const cell = event.target;
    if (cell.classList.contains("editing")) return;
    
    const id = cell.dataset.id;
    const field = cell.dataset.field;
    const value = cell.textContent;
    
    if (!id) return;
    
    cell.classList.add("editing");
    const input = document.createElement("input");
    input.type = field === "amount" ? "number" : "text";
    input.value = value;
    input.style.width = "100%";
    
    cell.textContent = "";
    cell.appendChild(input);
    input.focus();
    input.select();
    
    function save() {
        const newValue = input.value;
        if (newValue && newValue !== value) {
            updateResponse(id, field, newValue);
        } else {
            cell.textContent = value;
        }
        cell.classList.remove("editing");
    }
    
    input.addEventListener("blur", save);
    input.addEventListener("keypress", (e) => {
        if (e.key === "Enter") save();
        if (e.key === "Escape") {
            cell.textContent = value;
            cell.classList.remove("editing");
        }
    });
    input.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            cell.textContent = value;
            cell.classList.remove("editing");
        }
    });
}

function handleExpenseCellClick(event) {
    const cell = event.target;
    if (cell.classList.contains("editing")) return;
    
    const id = cell.dataset.expenseId;
    const field = cell.dataset.field;
    const value = cell.textContent;
    
    if (!id) return;
    
    cell.classList.add("editing");
    const input = document.createElement("input");
    input.type = field === "amount" ? "number" : "text";
    input.value = value;
    input.step = field === "amount" ? "0.01" : null;
    input.style.width = "100%";
    
    cell.textContent = "";
    cell.appendChild(input);
    input.focus();
    input.select();
    
    function save() {
        const newValue = input.value;
        if (newValue && newValue !== value) {
            updateExpense(id, field, newValue);
        } else {
            cell.textContent = value;
        }
        cell.classList.remove("editing");
    }
    
    input.addEventListener("blur", save);
    input.addEventListener("keypress", (e) => {
        if (e.key === "Enter") save();
        if (e.key === "Escape") {
            cell.textContent = value;
            cell.classList.remove("editing");
        }
    });
    input.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            cell.textContent = value;
            cell.classList.remove("editing");
        }
    });
}

function updateResponse(id, field, newValue) {
    fetch(`http://localhost:5000/api/bot/response/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            userResponse: newValue
        })
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            console.log("Response updated:", data);
            loadResponses();
        })
        .catch(err => {
            console.error("Failed to update response:", err);
            alert("Failed to update. Please try again.");
            loadResponses();
        });
}

function deleteResponse(id) {
    if (!confirm("Are you sure you want to delete this response?")) return;
    
    fetch(`http://localhost:5000/api/bot/response/${id}`, {
        method: "DELETE"
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        })
        .then(() => {
            console.log("Response deleted");
            loadResponses();
        })
        .catch(err => {
            console.error("Failed to delete response:", err);
            alert("Failed to delete. Please try again.");
        });
}

function updateExpense(id, field, newValue) {
    const expense = allExpenses.find(e => e.id === parseInt(id));
    if (!expense) return;
    
    const updated = {
        label: field === "label" ? newValue : expense.label,
        amount: field === "amount" ? parseFloat(newValue) : expense.amount,
        paidBy: field === "paidBy" ? newValue : expense.paidBy
    };
    
    fetch(`http://localhost:5000/api/bot/expense/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updated)
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            console.log("Expense updated:", data);
            loadExpenses();
        })
        .catch(err => {
            console.error("Failed to update expense:", err);
            alert("Failed to update. Please try again.");
            loadExpenses();
        });
}

function deleteExpense(id) {
    if (!confirm("Are you sure you want to delete this expense?")) return;
    
    fetch(`http://localhost:5000/api/bot/expense/${id}`, {
        method: "DELETE"
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        })
        .then(() => {
            console.log("Expense deleted");
            loadExpenses();
        })
        .catch(err => {
            console.error("Failed to delete expense:", err);
            alert("Failed to delete. Please try again.");
        });
}

// Bot functions
function startExpenseMode() {
    expenseMode = true;
    currentPersonType = null;
    currentExpense = {};
    
    // Show help message
    addChatMessage("🤖 Bot", t("bot.welcome"), "🤖", new Date().toISOString());
    
    // Start asking for the first expense
    setTimeout(() => {
        fetch("http://localhost:5000/api/bot/next-question/shared")
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(data => {
                currentBotQuestion = data;
                isBotWaiting = true;
                addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
                document.getElementById("message-input").placeholder = "Enter expense label...";
            })
            .catch(err => {
                console.error("Failed to get expense question:", err);
                addChatMessage("🤖 Bot", t("bot.error"), "🤖", new Date().toISOString());
            });
    }, 500);
}

function askWomanQuestions() {
    currentPersonType = "woman";
    askNextQuestion();
}

function askManQuestions() {
    currentPersonType = "man";
    askNextQuestion();
}

function askNextQuestion() {
    const url = currentPersonType 
        ? `http://localhost:5000/api/bot/next-question/${currentPersonType}`
        : "http://localhost:5000/api/bot/next-question";
    
    fetch(url)
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            currentBotQuestion = data;
            isBotWaiting = true;
            addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
            document.getElementById("message-input").placeholder = "Respond to bot...";
        })
        .catch(err => {
            console.error("Failed to get question:", err);
            const firstNames = SyncManager.getFirstNames();
            const womanName = firstNames["woman"] || "Woman";
            const manName = firstNames["man"] || "Man";
            if (currentPersonType === "woman") {
                addChatMessage("🤖 Bot", `All questions for ${womanName} have been answered. Click 'Man's Questions' to continue.`, "🤖", new Date().toISOString());
            } else if (currentPersonType === "man") {
                addChatMessage("🤖 Bot", `All questions for ${manName} have been answered. Thank you!`, "🤖", new Date().toISOString());
            }
            currentPersonType = null;
            isBotWaiting = false;
        });
}

function askBotQuestion() {
    fetch("http://localhost:5000/api/bot/next-question")
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            currentBotQuestion = data;
            isBotWaiting = true;
            addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
            document.getElementById("message-input").placeholder = "Respond to bot...";
        })
        .catch(err => {
            console.error("Failed to get bot question:", err);
        });
}

function respondToBot(response) {
    if (!currentBotQuestion) return;

    // Check if this is a "What is your first name?" question and extract the name
    if (currentBotQuestion.text && currentBotQuestion.text.includes("What is your first name?")) {
        SyncManager.saveFirstName(currentPersonType, response);
        console.log(`Saved first name for ${currentPersonType}: ${response}`);
    }

    // Handle expense questions
    if (currentBotQuestion.category === "expenses") {
        if (currentBotQuestion.text.includes("expense label")) {
            currentExpense.label = response;
        } else if (currentBotQuestion.text.includes("monthly amount")) {
            currentExpense.amount = response;
        } else if (currentBotQuestion.text.includes("Who paid")) {
            currentExpense.paidBy = response;
            
            // Save the complete expense
            if (currentExpense.label && currentExpense.amount && currentExpense.paidBy) {
                saveExpense(currentExpense.label, currentExpense.amount, currentExpense.paidBy);
                addChatMessage("🤖 Bot", t("bot.expenseRecorded", { label: currentExpense.label, amount: currentExpense.amount, paidBy: currentExpense.paidBy }), "🤖", new Date().toISOString());
                currentExpense = {}; // Reset for next expense
                isBotWaiting = false;
                currentBotQuestion = null;
                document.getElementById("message-input").placeholder = "Type your message...";
                return;
            }
        }
        
        // Add user response to chat
        addChatMessage(currentUser, response, getAvatarForUser(currentUser), new Date().toISOString());
        
        // Ask next expense question
        isBotWaiting = false;
        currentBotQuestion = null;
        document.getElementById("message-input").placeholder = "Type your message...";
        setTimeout(() => {
            fetch("http://localhost:5000/api/bot/next-question/shared")
                .then(res => {
                    if (!res.ok) throw new Error(`HTTP ${res.status}`);
                    return res.json();
                })
                .then(data => {
                    currentBotQuestion = data;
                    isBotWaiting = true;
                    addChatMessage("🤖 Bot", data.text, "🤖", new Date().toISOString());
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
            questionId: currentBotQuestion.id,
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
            addChatMessage(currentUser, response, getAvatarForUser(currentUser), new Date().toISOString());
            
            // Refresh dashboard table with new response
            loadResponses();
            
            // Reset bot state
            isBotWaiting = false;
            currentBotQuestion = null;
            document.getElementById("message-input").placeholder = "Type your message...";
            
            // If in question mode, ask the next question
            if (currentPersonType) {
                setTimeout(askNextQuestion, 500);
            }
        })
        .catch(err => {
            console.error("Failed to respond to bot:", err);
            isBotWaiting = false;
            currentBotQuestion = null;
            document.getElementById("message-input").placeholder = "Type your message...";
        });
}

// Initialize on page load
document.addEventListener("DOMContentLoaded", () => {
    currentUser = SyncManager.getSender();

    document.getElementById("message-input").addEventListener("keypress", (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // Wire up the woman and man buttons
    const womanBtn = document.getElementById("woman-btn");
    const manBtn = document.getElementById("man-btn");
    const expensesBtn = document.getElementById("expenses-btn");
    
    if (womanBtn) {
        womanBtn.onclick = askWomanQuestions;
        womanBtn.style.padding = "6px 12px";
        womanBtn.style.border = "none";
        womanBtn.style.borderRadius = "4px";
        womanBtn.style.cursor = "pointer";
        womanBtn.style.fontSize = "12px";
    }
    
    if (manBtn) {
        manBtn.onclick = askManQuestions;
        manBtn.style.padding = "6px 12px";
        manBtn.style.border = "none";
        manBtn.style.borderRadius = "4px";
        manBtn.style.cursor = "pointer";
        manBtn.style.fontSize = "12px";
    }
    
    if (expensesBtn) {
        expensesBtn.onclick = startExpenseMode;
        expensesBtn.style.padding = "6px 12px";
        expensesBtn.style.border = "none";
        expensesBtn.style.borderRadius = "4px";
        expensesBtn.style.cursor = "pointer";
        expensesBtn.style.fontSize = "12px";
    }
    
    // Initialize button labels with first names if available
    updateButtonLabels();

    startPolling();
});
