/**
 * Chatbot UI and interaction logic for the FredAuto web application.
 * Handles user input, message rendering, and communication with the backend AI API.
 *
 * Dependencies: None (uses vanilla JS and Fetch API)
 */
document.addEventListener('DOMContentLoaded', function () {
    const chatWindow = document.getElementById('chat-window');
    const chatbotBubble = document.getElementById('chatbot-bubble');
    const closeChatBtn = document.getElementById('close-chat-btn');
    const sendChatBtn = document.getElementById('send-chat-btn');
    const chatInput = document.getElementById('chat-input');
    const chatMessages = document.getElementById('chat-messages');


    /**
     * Stores the chat history for context in AI requests.
     * @type {Array<{role: string, text: string}>}
     */
    let conversationHistory = [];

    // --- UI Functions ---
    /**
     * Adds a message to the chat window and updates the conversation history.
     * @param {string} message - The message text to display.
     * @param {string} sender - 'user' or 'bot'.
     * @param {object|null} [toolData] - Optional structured data to render (list, link, etc.).
     */
    function addMessage(message, sender, toolData = null) {
        const messageElement = document.createElement('div');
        messageElement.classList.add('chat-message', `${sender}-message`);

        // Check if there is structured tool data to render
        if (toolData && toolData.type) {
            let content = `<p class="mb-2">${message}</p>`; // Main text part
            if (toolData.type === 'list') {
                content += '<ul class="list-group list-group-flush">';
                toolData.items.forEach(item => {
                    content += `<li class="list-group-item">${item}</li>`;
                });
                content += '</ul>';
            } else if (toolData.type === 'link') {
                content += `<a href="${toolData.url}" class="btn btn-primary btn-sm mt-2">${toolData.text}</a>`;
            }
            messageElement.innerHTML = content;
        } else {
            // Otherwise, just render plain text to prevent HTML injection
            messageElement.innerText = message;
        }

        chatMessages.appendChild(messageElement);

        // Add message to our history array. Use the 'model' role for the bot.
        conversationHistory.push({ role: sender === 'user' ? 'user' : 'model', text: message });

        // Scroll to the bottom of the chat window
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // --- AI Interaction Function ---
    /**
     * Sends the user's input to the backend AI API and displays the response.
     * Shows a temporary 'thinking' message while waiting for the response.
     * @param {string} userInput - The user's message to send to the AI.
     */
    async function getAIResponse(userInput) {
        // Show a temporary "thinking" message with a spinner
        const thinkingElement = document.createElement('div');
        thinkingElement.classList.add('chat-message', 'bot-message', 'thinking');
        thinkingElement.innerHTML = `
            <div class="d-flex align-items-center">
                <span class="spinner-grow spinner-grow-sm me-2" role="status" aria-hidden="true"></span>
                <span>Thinking...</span>
            </div>`;
        chatMessages.appendChild(thinkingElement);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        try {
            const response = await fetch('/api/chatbot/ask', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    newMessage: userInput,
                    // Send the last 10 messages (5 pairs of user/bot) for context
                    history: conversationHistory.slice(-10)
                })
            });

            // Always remove the "thinking" message regardless of the outcome
            thinkingElement.remove();

            if (!response.ok) {
                const error = await response.text();
                addMessage(`Oops! I encountered an error. The server said: ${error}`, 'bot');
                return;
            }

            const data = await response.json();
            // Pass both the text reply and the structured tool result to addMessage
            addMessage(data.reply, 'bot', data.toolResult);

        } catch (error) {
            thinkingElement.remove();
            addMessage('I seem to be having trouble connecting to my brain. Please check your internet connection and try again.', 'bot');
            console.error('Chatbot API call failed:', error);
        }
    }

    /**
     * Handles sending a message when the user clicks send or presses Enter.
     */
    function handleSendMessage() {
        const userInput = chatInput.value.trim();
        if (userInput === '') return;

        addMessage(userInput, 'user');
        chatInput.value = '';

        // Get the AI's response
        getAIResponse(userInput);
    }

    // --- Event Listeners ---
    // --- Event Listeners ---

    /**
     * Shows or hides the chat window when the chatbot bubble is clicked.
     */
    chatbotBubble.addEventListener('click', () => {
        chatWindow.classList.toggle('d-none');
        setTimeout(() => chatWindow.classList.toggle('visible'), 10);
    });

    /**
     * Closes the chat window when the close button is clicked.
     */
    closeChatBtn.addEventListener('click', () => {
        chatWindow.classList.remove('visible');
        setTimeout(() => chatWindow.classList.add('d-none'), 300); // Wait for animation to finish
    });

    /**
     * Sends the user's message when the send button is clicked.
     */
    sendChatBtn.addEventListener('click', handleSendMessage);

    /**
     * Sends the user's message when Enter is pressed in the input field.
     */
    chatInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            handleSendMessage();
        }
    });

    // --- Initial Bot Greeting ---
    /**
     * Displays the initial greeting from the chatbot after a short delay.
     */
    setTimeout(() => {
        addMessage("Hello! I'm Fred, the FredAuto AI Assistant. How can I help you today?", 'bot');
    }, 1000);
});