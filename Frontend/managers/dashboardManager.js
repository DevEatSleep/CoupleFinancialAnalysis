// dashboardManager.js - Handles dashboard responses/personal information

let allResponses = {};

const DashboardManager = {
    loadResponses() {
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
                    const category = resp.category || resp.Category || "";
                    const questionId = resp.questionId || resp.QuestionId || "";
                    const userResponse = resp.userResponse || resp.UserResponse || "";
                    const respId = resp.id || resp.Id;
                    
                    if (personValue === "woman" || personValue === "man") {
                        // Use category and questionId to determine field type (language-independent)
                        if (category === "personal") {
                            if (questionId === 1 || questionId === 4) {
                                // Question 1 is woman's first name, question 4 is man's first name
                                allResponses[personValue].name = userResponse;
                                allResponses[personValue].nameId = respId;
                            } else if (questionId === 2 || questionId === 5) {
                                // Question 2 is woman's age, question 5 is man's age
                                allResponses[personValue].age = userResponse;
                                allResponses[personValue].ageId = respId;
                            }
                        } else if (category === "financial") {
                            if (questionId === 3 || questionId === 6) {
                                // Question 3 is woman's salary, question 6 is man's salary
                                allResponses[personValue].salary = userResponse;
                                allResponses[personValue].salaryId = respId;
                            }
                        }
                    }
                });
                console.log("Organized allResponses:", allResponses);
                this.updateDashboardTable();
            })
            .catch(err => {
                console.error("Failed to load responses:", err);
            });
    },

    updateDashboardTable() {
        const tbody = document.getElementById("responses-tbody");
        console.log("tbody element:", tbody);
        if (!tbody) {
            console.error("responses-tbody element not found! Retrying...");
            setTimeout(() => this.updateDashboardTable(), 100);
            return;
        }

        console.log("allResponses:", allResponses);
        
        // Check if there's any actual data (not just "-" placeholders)
        const hasWomanData = allResponses && allResponses.woman && (allResponses.woman.name !== "-" || allResponses.woman.age !== "-" || allResponses.woman.salary !== "-");
        const hasManData = allResponses && allResponses.man && (allResponses.man.name !== "-" || allResponses.man.age !== "-" || allResponses.man.salary !== "-");
        
        console.log("hasWomanData:", hasWomanData, "hasManData:", hasManData);
        
        if (!hasWomanData && !hasManData) {
            console.log("Setting empty state message");
            tbody.innerHTML = '<tr><td colspan="5" class="empty-state">No responses yet</td></tr>';
            return;
        }

        tbody.innerHTML = "";
        const firstNames = SyncManager.getFirstNames();
        
        console.log("Creating rows for woman and man");
        
        // Add woman row
        if (allResponses.woman) {
            const womanRow = document.createElement("tr");
            const womanName = firstNames["woman"] || allResponses.woman.name;
            womanRow.innerHTML = `
                <td><strong>👩 Woman</strong></td>
                <td class="editable" data-id="${allResponses.woman.nameId}" data-field="name">${this.escapeHtml(womanName)}</td>
                <td class="editable" data-id="${allResponses.woman.ageId}" data-field="age">${this.escapeHtml(allResponses.woman.age)}</td>
                <td class="editable" data-id="${allResponses.woman.salaryId}" data-field="salary">${this.escapeHtml(allResponses.woman.salary)}</td>
                <td><button class="delete-btn" onclick="DashboardManager.deleteResponse(${allResponses.woman.nameId})">🗑️</button></td>
            `;
            womanRow.querySelectorAll(".editable").forEach(cell => {
                cell.addEventListener("click", (e) => this.handleCellClick(e));
            });
            tbody.appendChild(womanRow);
            console.log("Woman row added");
        }
        
        // Add man row
        if (allResponses.man) {
            const manRow = document.createElement("tr");
            const manName = firstNames["man"] || allResponses.man.name;
            manRow.innerHTML = `
                <td><strong>👨 Man</strong></td>
                <td class="editable" data-id="${allResponses.man.nameId}" data-field="name">${this.escapeHtml(manName)}</td>
                <td class="editable" data-id="${allResponses.man.ageId}" data-field="age">${this.escapeHtml(allResponses.man.age)}</td>
                <td class="editable" data-id="${allResponses.man.salaryId}" data-field="salary">${this.escapeHtml(allResponses.man.salary)}</td>
                <td><button class="delete-btn" onclick="DashboardManager.deleteResponse(${allResponses.man.nameId})">🗑️</button></td>
            `;
            manRow.querySelectorAll(".editable").forEach(cell => {
                cell.addEventListener("click", (e) => this.handleCellClick(e));
            });
            tbody.appendChild(manRow);
            console.log("Man row added");
        }
        
        // Update button labels with first names
        UIManager.updateButtonLabels();
    },

    handleCellClick(event) {
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
        
        const save = () => {
            const newValue = input.value;
            if (newValue && newValue !== value) {
                this.updateResponse(id, field, newValue);
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

    updateResponse(id, field, newValue) {
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
                this.loadResponses();
            })
            .catch(err => {
                console.error("Failed to update response:", err);
                alert("Failed to update. Please try again.");
                this.loadResponses();
            });
    },

    deleteResponse(id) {
        if (!confirm("Are you sure you want to delete this response?")) return;
        
        fetch(`http://localhost:5000/api/bot/response/${id}`, {
            method: "DELETE"
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
            })
            .then(() => {
                console.log("Response deleted");
                this.loadResponses();
            })
            .catch(err => {
                console.error("Failed to delete response:", err);
                alert("Failed to delete. Please try again.");
            });
    },

    escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
};
