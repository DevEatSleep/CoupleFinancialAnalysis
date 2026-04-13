// LocalStorage sync management
const STORAGE_KEY = "couple_chat_messages";
const SENDER_KEY = "couple_chat_sender";
const PENDING_QUEUE_KEY = "couple_chat_pending";
const FIRST_NAMES_KEY = "couple_chat_first_names";

class SyncManager {
    static saveMessage(sender, content, avatarUrl) {
        const messages = this.getMessages();
        const message = {
            id: Date.now(),
            sender,
            content,
            avatarUrl,
            createdAt: new Date().toISOString(),
            synced: false
        };
        messages.push(message);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(messages));
        return message;
    }

    static getMessages() {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored ? JSON.parse(stored) : [];
    }

    static markMessageSynced(messageId) {
        const messages = this.getMessages();
        const msg = messages.find(m => m.id === messageId);
        if (msg) {
            msg.synced = true;
            localStorage.setItem(STORAGE_KEY, JSON.stringify(messages));
        }
    }

    static getPendingMessages() {
        const messages = this.getMessages();
        return messages.filter(m => !m.synced);
    }

    static saveSender(sender) {
        localStorage.setItem(SENDER_KEY, sender);
    }

    static getSender() {
        return localStorage.getItem(SENDER_KEY) || "Person1";
    }

    static clearMessages() {
        localStorage.removeItem(STORAGE_KEY);
    }

    static addToPendingQueue(messageId) {
        const queue = JSON.parse(localStorage.getItem(PENDING_QUEUE_KEY) || "[]");
        queue.push(messageId);
        localStorage.setItem(PENDING_QUEUE_KEY, JSON.stringify(queue));
    }

    static getPendingQueue() {
        return JSON.parse(localStorage.getItem(PENDING_QUEUE_KEY) || "[]");
    }

    static removeFromPendingQueue(messageId) {
        const queue = JSON.parse(localStorage.getItem(PENDING_QUEUE_KEY) || "[]");
        const filtered = queue.filter(id => id !== messageId);
        localStorage.setItem(PENDING_QUEUE_KEY, JSON.stringify(filtered));
    }

    static saveMessages(messages) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(messages));
    }

    static saveFirstName(sender, firstName) {
        const firstNames = JSON.parse(localStorage.getItem(FIRST_NAMES_KEY) || "{}");
        firstNames[sender] = firstName;
        localStorage.setItem(FIRST_NAMES_KEY, JSON.stringify(firstNames));
    }

    static getFirstName(sender) {
        const firstNames = JSON.parse(localStorage.getItem(FIRST_NAMES_KEY) || "{}");
        return firstNames[sender] || null;
    }

    static getFirstNames() {
        return JSON.parse(localStorage.getItem(FIRST_NAMES_KEY) || "{}");
    }

    static clearFirstNames() {
        localStorage.removeItem(FIRST_NAMES_KEY);
    }
}
