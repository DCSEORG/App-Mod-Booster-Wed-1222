// Chat UI JavaScript

const chatMessages = document.getElementById('chatMessages');
const chatInput = document.getElementById('chatInput');
const sendButton = document.getElementById('sendButton');
const typingIndicator = document.querySelector('.typing-indicator');

// Check if GenAI is configured
let genAIConfigured = false;

// Initialize
async function init() {
    // Check if GenAI endpoint is configured
    try {
        const response = await fetch('/api/chat/status');
        const data = await response.json();
        genAIConfigured = data.configured;
        
        if (!genAIConfigured) {
            addMessage('assistant', '⚠️ GenAI services are not deployed. I can still help you with basic queries, but for the full AI experience, please run deploy-with-chat.sh to deploy Azure OpenAI services.');
        }
    } catch (error) {
        console.error('Error checking GenAI status:', error);
        addMessage('assistant', '⚠️ Unable to connect to the backend. Please ensure the application is running.');
    }
}

// Send message
async function sendMessage() {
    const message = chatInput.value.trim();
    if (!message) return;
    
    // Add user message
    addMessage('user', message);
    chatInput.value = '';
    
    // Disable input while processing
    sendButton.disabled = true;
    chatInput.disabled = true;
    typingIndicator.classList.add('active');
    
    try {
        // Call chat API
        const response = await fetch('/api/chat/message', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ message: message })
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        // Add assistant response
        addMessage('assistant', data.response);
        
    } catch (error) {
        console.error('Error:', error);
        addMessage('assistant', '❌ Sorry, I encountered an error processing your request. Please try again or check if the backend services are running.');
    } finally {
        // Re-enable input
        sendButton.disabled = false;
        chatInput.disabled = false;
        typingIndicator.classList.remove('active');
        chatInput.focus();
    }
}

// Add message to chat
function addMessage(role, content) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${role}`;
    
    const bubbleDiv = document.createElement('div');
    bubbleDiv.className = 'message-bubble';
    
    // Escape HTML and format content
    const formattedContent = formatMessage(escapeHtml(content));
    bubbleDiv.innerHTML = formattedContent;
    
    const timeDiv = document.createElement('div');
    timeDiv.className = 'message-time';
    timeDiv.textContent = new Date().toLocaleTimeString();
    
    messageDiv.appendChild(bubbleDiv);
    messageDiv.appendChild(timeDiv);
    
    // Insert before typing indicator
    chatMessages.insertBefore(messageDiv, typingIndicator);
    
    // Scroll to bottom
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Format message with markdown-like syntax
function formatMessage(text) {
    // Convert **bold** to <strong>
    text = text.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    
    // Convert numbered lists
    text = text.replace(/^\d+\.\s+(.+)$/gm, '<li>$1</li>');
    if (text.includes('<li>')) {
        text = text.replace(/(<li>.*<\/li>)/s, '<ol>$1</ol>');
    }
    
    // Convert bullet lists
    text = text.replace(/^[-*]\s+(.+)$/gm, '<li>$1</li>');
    if (text.includes('<li>') && !text.includes('<ol>')) {
        text = text.replace(/(<li>.*<\/li>)/s, '<ul>$1</ul>');
    }
    
    // Convert newlines to <br>
    text = text.replace(/\n/g, '<br>');
    
    return text;
}

// Handle key press
function handleKeyPress(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendMessage();
    }
}

// Send suggestion
function sendSuggestion(text) {
    chatInput.value = text;
    sendMessage();
}

// Initialize on load
init();
