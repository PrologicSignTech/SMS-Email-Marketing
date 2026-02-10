/**
 * Chat Widget JavaScript - Handles SignalR real-time communication
 * Position: Bottom LEFT corner of the page
 */

(function() {
    'use strict';

    // Chat Widget State
    const chatState = {
        connection: null,
        chatRoomId: null,
        guestName: null,
        guestEmail: null,
        isConnected: false,
        typingTimeout: null
    };

    // Toast notification system
    const toast = {
        container: null,

        init() {
            // Create toast container if it doesn't exist
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'chat-toast-container';
                this.container.style.cssText = `
                    position: fixed;
                    bottom: 100px;
                    left: 20px;
                    z-index: 10000;
                    display: flex;
                    flex-direction: column;
                    gap: 10px;
                    max-width: 350px;
                `;
                document.body.appendChild(this.container);
            }
        },

        show(message, type = 'error') {
            this.init();

            const toastEl = document.createElement('div');
            toastEl.className = `chat-toast chat-toast-${type}`;

            const colors = {
                error: { bg: '#fee2e2', border: '#ef4444', text: '#991b1b', icon: 'bi-exclamation-circle-fill' },
                success: { bg: '#dcfce7', border: '#22c55e', text: '#166534', icon: 'bi-check-circle-fill' },
                warning: { bg: '#fef3c7', border: '#f59e0b', text: '#92400e', icon: 'bi-exclamation-triangle-fill' },
                info: { bg: '#dbeafe', border: '#3b82f6', text: '#1e40af', icon: 'bi-info-circle-fill' }
            };

            const style = colors[type] || colors.error;

            toastEl.style.cssText = `
                background: ${style.bg};
                border: 1px solid ${style.border};
                border-left: 4px solid ${style.border};
                color: ${style.text};
                padding: 12px 16px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                display: flex;
                align-items: center;
                gap: 10px;
                animation: chatToastSlideIn 0.3s ease;
                font-size: 14px;
                font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            `;

            toastEl.innerHTML = `
                <i class="bi ${style.icon}" style="font-size: 18px;"></i>
                <span style="flex: 1;">${message}</span>
                <button onclick="this.parentElement.remove()" style="background: none; border: none; cursor: pointer; padding: 0; color: ${style.text}; opacity: 0.7;">
                    <i class="bi bi-x" style="font-size: 18px;"></i>
                </button>
            `;

            this.container.appendChild(toastEl);

            // Auto remove after 5 seconds
            setTimeout(() => {
                if (toastEl.parentElement) {
                    toastEl.style.animation = 'chatToastSlideOut 0.3s ease forwards';
                    setTimeout(() => toastEl.remove(), 300);
                }
            }, 5000);
        },

        error(message) { this.show(message, 'error'); },
        success(message) { this.show(message, 'success'); },
        warning(message) { this.show(message, 'warning'); },
        info(message) { this.show(message, 'info'); }
    };

    // Add toast animations to document
    const styleSheet = document.createElement('style');
    styleSheet.textContent = `
        @keyframes chatToastSlideIn {
            from { opacity: 0; transform: translateX(-20px); }
            to { opacity: 1; transform: translateX(0); }
        }
        @keyframes chatToastSlideOut {
            from { opacity: 1; transform: translateX(0); }
            to { opacity: 0; transform: translateX(-20px); }
        }
    `;
    document.head.appendChild(styleSheet);

    // DOM Elements
    const elements = {
        chatWidget: document.getElementById('chat-widget'),
        chatButton: document.getElementById('chat-button'),
        chatWindow: document.getElementById('chat-window'),
        chatCloseBtn: document.getElementById('chat-close-btn'),
        preChatForm: document.getElementById('pre-chat-form'),
        startChatBtn: document.getElementById('start-chat-btn'),
        guestNameInput: document.getElementById('guest-name'),
        guestEmailInput: document.getElementById('guest-email'),
        chatMessagesArea: document.getElementById('chat-messages-area'),
        chatMessages: document.getElementById('chat-messages'),
        chatInput: document.getElementById('chat-input'),
        sendMessageBtn: document.getElementById('send-message-btn'),
        typingIndicator: document.getElementById('typing-indicator'),
        unreadBadge: document.getElementById('chat-unread-badge')
    };

    // Initialize Chat Widget
    function initChatWidget() {
        console.log('Initializing chat widget...');
        
        // Check if required elements exist
        if (!elements.chatButton || !elements.chatWindow) {
            console.warn('Chat widget elements not found');
            return;
        }
        
        // Event Listeners
        if (elements.chatButton) elements.chatButton.addEventListener('click', toggleChatWindow);
        if (elements.chatCloseBtn) elements.chatCloseBtn.addEventListener('click', closeChatWindow);
        if (elements.startChatBtn) elements.startChatBtn.addEventListener('click', handleStartChat);
        if (elements.sendMessageBtn) elements.sendMessageBtn.addEventListener('click', sendMessage);
        if (elements.chatInput) {
            elements.chatInput.addEventListener('keydown', handleChatInputKeydown);
            elements.chatInput.addEventListener('input', handleTyping);
        }

        // Check if there's a saved chat session
        checkSavedSession();
    }

    // Toggle chat window
    function toggleChatWindow() {
        if (!elements.chatWindow) return;
        
        elements.chatWindow.classList.toggle('show');
        if (elements.chatWindow.classList.contains('show')) {
            // Reset unread count
            if (elements.unreadBadge) {
                elements.unreadBadge.style.display = 'none';
                elements.unreadBadge.textContent = '0';
            }
        }
    }

    // Close chat window
    function closeChatWindow() {
        if (!elements.chatWindow) return;
        elements.chatWindow.classList.remove('show');
    }

    // Check for saved session in localStorage
    function checkSavedSession() {
        const savedChatRoomId = localStorage.getItem('chatRoomId');
        const savedGuestName = localStorage.getItem('guestName');
        const savedGuestEmail = localStorage.getItem('guestEmail');

        if (savedChatRoomId && savedGuestName && savedGuestEmail) {
            chatState.chatRoomId = parseInt(savedChatRoomId);
            chatState.guestName = savedGuestName;
            chatState.guestEmail = savedGuestEmail;

            // Show chat messages area
            elements.preChatForm.style.display = 'none';
            elements.chatMessagesArea.classList.add('show');

            // Connect to SignalR and load chat history
            connectToHub();
            loadChatHistory();
        }
    }

    // Handle Start Chat
    async function handleStartChat() {
        const name = elements.guestNameInput.value.trim();
        const email = elements.guestEmailInput.value.trim();

        // Validation
        if (!name || !email) {
            toast.warning('Please enter your name and email');
            return;
        }

        if (!isValidEmail(email)) {
            toast.warning('Please enter a valid email address');
            return;
        }

        // Disable button
        elements.startChatBtn.disabled = true;
        elements.startChatBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Starting...';

        try {
            // Create chat room via API using AppUrls
            const apiUrl = window.AppUrls ? window.AppUrls.buildApiUrl(window.AppUrls.api.chat.createRoom) : '/api/chat/rooms';
            console.log('Chat API URL:', apiUrl);

            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    guestName: name,
                    guestEmail: email
                })
            });

            console.log('Chat API Response Status:', response.status);

            // Handle non-OK responses
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Chat API Error Response:', errorText);
                throw new Error(`Server error: ${response.status} - ${errorText || 'Unknown error'}`);
            }

            const result = await response.json();
            console.log('Chat API Result:', result);

            if (result.success && result.data) {
                chatState.chatRoomId = result.data.id;
                chatState.guestName = name;
                chatState.guestEmail = email;

                // Save to localStorage
                localStorage.setItem('chatRoomId', chatState.chatRoomId);
                localStorage.setItem('guestName', name);
                localStorage.setItem('guestEmail', email);

                // Hide pre-chat form and show chat area
                elements.preChatForm.style.display = 'none';
                elements.chatMessagesArea.classList.add('show');

                // Connect to SignalR hub
                await connectToHub();

                // Add welcome message
                addSystemMessage(`Welcome, ${name}! A support agent will be with you shortly.`);
                toast.success('Chat started successfully!');
            } else {
                throw new Error(result.message || 'Failed to start chat');
            }
        } catch (error) {
            console.error('Error starting chat:', error);
            toast.error(`Failed to start chat: ${error.message || 'Please try again.'}`);
        } finally {
            elements.startChatBtn.disabled = false;
            elements.startChatBtn.innerHTML = '<i class="bi bi-chat-dots"></i> Start Chat';
        }
    }

    // Connect to SignalR Hub
    async function connectToHub() {
        if (chatState.isConnected) {
            console.log('Already connected to hub');
            return;
        }

        try {
            // Build SignalR hub URL using dedicated hub helper
            const hubUrl = window.AppUrls ? window.AppUrls.buildHubUrl(window.AppUrls.hubs.chat) : '/hubs/chat';
            
            chatState.connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
                .withAutomaticReconnect()
                .build();

            // Set up event handlers
            setupSignalRHandlers();

            // Start connection
            await chatState.connection.start();
            console.log('Connected to chat hub');
            chatState.isConnected = true;

            // Join the chat room
            await chatState.connection.invoke('JoinChatRoom', chatState.chatRoomId);
        } catch (error) {
            console.error('Error connecting to hub:', error);
            addSystemMessage('Connection error. Trying to reconnect...');
        }
    }

    // Setup SignalR event handlers
    function setupSignalRHandlers() {
        // Receive message
        chatState.connection.on('ReceiveMessage', (message) => {
            console.log('Message received:', message);
            addChatMessage(message);
            
            // Play notification sound if window is not visible
            if (!elements.chatWindow.classList.contains('show')) {
                incrementUnreadCount();
            }
        });

        // User typing
        chatState.connection.on('UserTyping', (userId, isTyping) => {
            if (isTyping) {
                elements.typingIndicator.classList.add('show');
            } else {
                elements.typingIndicator.classList.remove('show');
            }
        });

        // Chat room closed
        chatState.connection.on('ChatRoomClosed', (chatRoomId) => {
            if (chatRoomId === chatState.chatRoomId) {
                addSystemMessage('Chat session has been closed. You will receive a transcript via email.');
                elements.chatInput.disabled = true;
                elements.sendMessageBtn.disabled = true;
                
                // Clear localStorage
                clearChatSession();
            }
        });

        // Connection closed
        chatState.connection.onclose(() => {
            console.log('Connection closed');
            chatState.isConnected = false;
            addSystemMessage('Connection lost. Reconnecting...');
        });

        // Reconnected
        chatState.connection.onreconnected(() => {
            console.log('Reconnected to hub');
            chatState.isConnected = true;
            addSystemMessage('Reconnected to chat');
            
            // Rejoin the chat room
            if (chatState.chatRoomId) {
                chatState.connection.invoke('JoinChatRoom', chatState.chatRoomId);
            }
        });
    }

    // Send message
    async function sendMessage() {
        const messageText = elements.chatInput.value.trim();
        
        if (!messageText || !chatState.isConnected) {
            return;
        }

        try {
            // Send via SignalR
            await chatState.connection.invoke('SendMessage', {
                chatRoomId: chatState.chatRoomId,
                messageText: messageText,
                messageType: 0 // Text message
            });

            // Clear input
            elements.chatInput.value = '';
            elements.chatInput.style.height = 'auto';
        } catch (error) {
            console.error('Error sending message:', error);
            addSystemMessage('Failed to send message. Please try again.');
        }
    }

    // Handle chat input keydown
    function handleChatInputKeydown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    }

    // Handle typing indicator
    function handleTyping() {
        // Auto-resize textarea
        elements.chatInput.style.height = 'auto';
        elements.chatInput.style.height = elements.chatInput.scrollHeight + 'px';

        if (!chatState.isConnected) return;

        // Clear previous typing timeout
        if (chatState.typingTimeout) {
            clearTimeout(chatState.typingTimeout);
        }

        // Notify typing
        chatState.connection.invoke('NotifyTyping', chatState.chatRoomId, true);

        // Stop typing after 3 seconds
        chatState.typingTimeout = setTimeout(() => {
            if (chatState.isConnected) {
                chatState.connection.invoke('NotifyTyping', chatState.chatRoomId, false);
            }
        }, 3000);
    }

    // Add chat message to UI
    function addChatMessage(message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-message ${message.isOwnMessage ? 'own' : 'other'}`;

        const bubble = document.createElement('div');
        bubble.className = 'message-bubble';

        if (!message.isOwnMessage) {
            const sender = document.createElement('div');
            sender.className = 'message-sender';
            sender.textContent = message.senderName || 'Support';
            bubble.appendChild(sender);
        }

        const text = document.createElement('div');
        text.className = 'message-text';
        text.textContent = message.messageText;
        bubble.appendChild(text);

        const time = document.createElement('div');
        time.className = 'message-time';
        time.textContent = formatTime(message.sentAt);
        bubble.appendChild(time);

        messageDiv.appendChild(bubble);
        elements.chatMessages.appendChild(messageDiv);

        // Scroll to bottom
        scrollToBottom();
    }

    // Add system message
    function addSystemMessage(message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'system-message';
        messageDiv.textContent = message;
        elements.chatMessages.appendChild(messageDiv);
        scrollToBottom();
    }

    // Load chat history
    async function loadChatHistory() {
        try {
            const apiUrl = window.AppUrls ? 
                window.AppUrls.buildApiUrl(window.AppUrls.api.chat.getMessages(chatState.chatRoomId)) : 
                `/api/chat/rooms/${chatState.chatRoomId}/messages`;
            const response = await fetch(apiUrl);
            const result = await response.json();

            if (result.success && result.data) {
                // Clear existing messages
                elements.chatMessages.innerHTML = '<div class="system-message">Chat session started</div>';
                
                // Add all messages
                result.data.forEach(message => {
                    addChatMessage(message);
                });
            }
        } catch (error) {
            console.error('Error loading chat history:', error);
        }
    }

    // Increment unread count
    function incrementUnreadCount() {
        if (!elements.unreadBadge) return;
        
        let count = parseInt(elements.unreadBadge.textContent) || 0;
        count++;
        elements.unreadBadge.textContent = count;
        elements.unreadBadge.style.display = 'flex';
    }

    // Scroll chat to bottom
    function scrollToBottom() {
        if (!elements.chatMessages) return;
        elements.chatMessages.scrollTop = elements.chatMessages.scrollHeight;
    }

    // Format time
    function formatTime(dateString) {
        const date = new Date(dateString);
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${hours}:${minutes}`;
    }

    // Validate email
    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    // Clear chat session
    function clearChatSession() {
        localStorage.removeItem('chatRoomId');
        localStorage.removeItem('guestName');
        localStorage.removeItem('guestEmail');
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initChatWidget);
    } else {
        initChatWidget();
    }
})();
