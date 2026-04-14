// expenseManager.js - Handles expense management

let allExpenses = [];
let currentExpense = {};
let expenseMode = false;

const ExpenseManager = {
    loadExpenses() {
        fetch("http://localhost:5000/api/bot/expenses")
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(expenses => {
                console.log("Loaded expenses:", expenses);
                allExpenses = expenses;
                this.updateExpensesTable();
            })
            .catch(err => {
                console.error("Failed to load expenses:", err);
            });
    },

    updateExpensesTable() {
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
                <td class="editable" data-expense-id="${expense.id}" data-field="label">${this.escapeHtml(expense.label)}</td>
                <td class="editable" data-expense-id="${expense.id}" data-field="amount">${expense.amount}</td>
                <td class="editable" data-expense-id="${expense.id}" data-field="paidBy">${this.escapeHtml(expense.paidBy)}</td>
                <td><button class="delete-btn" onclick="ExpenseManager.deleteExpense(${expense.id})">🗑️</button></td>
            `;
            row.querySelectorAll(".editable").forEach(cell => {
                cell.addEventListener("click", (e) => this.handleExpenseCellClick(e));
            });
            tbody.appendChild(row);
        });
    },

    saveExpense(label, amount, paidBy) {
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
                this.loadExpenses();
            })
            .catch(err => {
                console.error("Failed to save expense:", err);
            });
    },

    handleExpenseCellClick(event) {
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
        
        const save = () => {
            const newValue = input.value;
            if (newValue && newValue !== value) {
                this.updateExpense(id, field, newValue);
            } else {
                cell.textContent = value;
            }
            cell.classList.remove("editing");
        };
        
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
    },

    updateExpense(id, field, newValue) {
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
                this.loadExpenses();
            })
            .catch(err => {
                console.error("Failed to update expense:", err);
                alert("Failed to update. Please try again.");
                this.loadExpenses();
            });
    },

    deleteExpense(id) {
        if (!confirm("Are you sure you want to delete this expense?")) return;
        
        fetch(`http://localhost:5000/api/bot/expense/${id}`, {
            method: "DELETE"
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
            })
            .then(() => {
                console.log("Expense deleted");
                this.loadExpenses();
            })
            .catch(err => {
                console.error("Failed to delete expense:", err);
                alert("Failed to delete. Please try again.");
            });
    },

    escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
};
