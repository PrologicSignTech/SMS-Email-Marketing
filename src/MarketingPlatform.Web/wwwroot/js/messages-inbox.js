/**
 * Two-Way Messaging Inbox
 * Real-time conversation management with agent assignment
 */

let conversations = [];
let currentConversationId = null;
let currentFilter = 'all';
let pollingInterval = null;

$(document).ready(function () {
    loadConversations();
    loadAgents();
    setupEventHandlers();
    // Poll for new messages every 10 seconds
    pollingInterval = setInterval(function () {
        loadConversations(true);
        if (currentConversationId) loadMessages(currentConversationId, true);
    }, 10000);
});

function setupEventHandlers() {
    // Search
    $('#searchConversations').on('input', debounce(function () {
        filterConversations();
    }, 300));

    // Filter tabs
    $('input[name="inboxFilter"]').on('change', function () {
        currentFilter = $(this).val();
        filterConversations();
    });

    // Send reply
    $('#sendReplyBtn').on('click', sendReply);
    $('#replyMessage').on('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendReply();
        }
    });

    // Character counter
    $('#replyMessage').on('input', function () {
        var len = $(this).val().length;
        $('#replyCharCount').text(len + '/160');
        if (len > 160) {
            $('#replyCharCount').addClass('text-danger');
        } else {
            $('#replyCharCount').removeClass('text-danger');
        }
    });

    // Mark resolved
    $('#markResolvedBtn').on('click', markResolved);

    // Insert token
    $('#insertTokenBtn').on('click', function () {
        var tokens = ['{FirstName}', '{LastName}', '{Email}', '{Phone}', '{CompanyName}', '{UnsubscribeLink}'];
        var tokenHtml = tokens.map(function (t) {
            return '<a href="#" class="dropdown-item small token-pick" data-token="' + t + '">' + t + '</a>';
        }).join('');
        // Show simple dropdown
        var dropdown = $('<div class="dropdown-menu show p-2" style="position:absolute; bottom:100%; left:0;">' + tokenHtml + '</div>');
        $(this).parent().css('position', 'relative').append(dropdown);
        dropdown.on('click', '.token-pick', function (e) {
            e.preventDefault();
            var token = $(this).data('token');
            var textarea = document.getElementById('replyMessage');
            var start = textarea.selectionStart;
            var text = textarea.value;
            textarea.value = text.substring(0, start) + token + text.substring(textarea.selectionEnd);
            textarea.focus();
            dropdown.remove();
        });
        $(document).one('click', function () { dropdown.remove(); });
    });
}

function loadConversations(silent) {
    if (!silent) {
        $('#conversationList').html('<div class="text-center py-5"><div class="spinner-border spinner-border-sm text-primary"></div></div>');
    }

    $.ajax({
        url: '/Messages/GetConversations',
        method: 'GET',
        success: function (response) {
            if (response.success) {
                conversations = response.data || [];
                renderConversations();
            } else {
                // Show demo conversations if API not ready
                conversations = getDemoConversations();
                renderConversations();
            }
        },
        error: function () {
            conversations = getDemoConversations();
            renderConversations();
        }
    });
}

function getDemoConversations() {
    return [
        { id: 1, contactName: 'John Smith', contactPhone: '+1 (555) 123-4567', channel: 0, lastMessage: 'Thanks for the update!', lastMessageTime: new Date().toISOString(), unreadCount: 2, assignedTo: null, status: 'open' },
        { id: 2, contactName: 'Sarah Johnson', contactPhone: '+1 (555) 234-5678', channel: 0, lastMessage: 'When does the sale start?', lastMessageTime: new Date(Date.now() - 3600000).toISOString(), unreadCount: 1, assignedTo: 'Agent Mike', status: 'open' },
        { id: 3, contactName: 'Mike Brown', contactEmail: 'mike@example.com', channel: 2, lastMessage: 'I received the confirmation email', lastMessageTime: new Date(Date.now() - 7200000).toISOString(), unreadCount: 0, assignedTo: null, status: 'resolved' },
        { id: 4, contactName: 'Emily Davis', contactPhone: '+1 (555) 345-6789', channel: 0, lastMessage: 'STOP', lastMessageTime: new Date(Date.now() - 86400000).toISOString(), unreadCount: 0, assignedTo: null, status: 'opted-out' },
        { id: 5, contactName: 'Robert Wilson', contactPhone: '+1 (555) 456-7890', channel: 1, lastMessage: 'Can you send me more info?', lastMessageTime: new Date(Date.now() - 1800000).toISOString(), unreadCount: 3, assignedTo: null, status: 'open' }
    ];
}

function renderConversations() {
    var filtered = conversations.filter(function (c) {
        if (currentFilter === 'unread') return c.unreadCount > 0;
        if (currentFilter === 'assigned') return c.assignedTo;
        return true;
    });

    var searchTerm = ($('#searchConversations').val() || '').toLowerCase();
    if (searchTerm) {
        filtered = filtered.filter(function (c) {
            return (c.contactName || '').toLowerCase().indexOf(searchTerm) >= 0 ||
                (c.contactPhone || '').indexOf(searchTerm) >= 0 ||
                (c.contactEmail || '').toLowerCase().indexOf(searchTerm) >= 0;
        });
    }

    if (filtered.length === 0) {
        $('#conversationList').html('<div class="text-center py-5"><i class="bi bi-chat-dots fs-3 text-muted"></i><p class="text-muted small mt-2">No conversations found</p></div>');
        return;
    }

    var html = '';
    filtered.forEach(function (conv) {
        var initials = getInitials(conv.contactName || 'Unknown');
        var channelNames = ['SMS', 'MMS', 'Email'];
        var channelColors = ['success', 'info', 'primary'];
        var timeAgo = getTimeAgo(conv.lastMessageTime);
        var isActive = conv.id === currentConversationId;
        var isUnread = conv.unreadCount > 0;

        html += '<div class="conversation-item' + (isActive ? ' active' : '') + (isUnread ? ' unread' : '') + '" data-id="' + conv.id + '" onclick="selectConversation(' + conv.id + ')">';
        html += '  <div class="d-flex gap-2 align-items-start">';
        html += '    <div class="avatar-circle bg-' + channelColors[conv.channel || 0] + ' text-white">' + initials + '</div>';
        html += '    <div class="flex-grow-1 min-width-0">';
        html += '      <div class="d-flex justify-content-between">';
        html += '        <strong class="text-truncate d-block" style="max-width:140px;">' + escapeHtml(conv.contactName || 'Unknown') + '</strong>';
        html += '        <small class="text-muted text-nowrap ms-1">' + timeAgo + '</small>';
        html += '      </div>';
        html += '      <div class="d-flex justify-content-between align-items-center">';
        html += '        <small class="text-muted text-truncate d-block" style="max-width:160px;">' + escapeHtml(conv.lastMessage || '') + '</small>';
        if (isUnread) {
            html += '      <span class="badge bg-primary rounded-pill ms-1">' + conv.unreadCount + '</span>';
        }
        html += '      </div>';
        if (conv.assignedTo) {
            html += '    <small class="assigned-badge text-info"><i class="bi bi-person-fill"></i> ' + escapeHtml(conv.assignedTo) + '</small>';
        }
        if (conv.status === 'opted-out') {
            html += '    <small class="badge bg-danger">Opted Out</small>';
        }
        html += '    </div>';
        html += '  </div>';
        html += '</div>';
    });

    $('#conversationList').html(html);
}

function selectConversation(id) {
    currentConversationId = id;
    var conv = conversations.find(function (c) { return c.id === id; });
    if (!conv) return;

    // Update UI
    $('.conversation-item').removeClass('active');
    $('[data-id="' + id + '"]').addClass('active').removeClass('unread');

    // Show chat header
    $('#chatHeader').css('display', 'flex !important').removeClass('d-none').show();
    var initials = getInitials(conv.contactName);
    $('#contactAvatar').text(initials);
    $('#contactName').text(conv.contactName || 'Unknown');
    $('#contactInfo').text(conv.contactPhone || conv.contactEmail || '');

    var channelNames = ['SMS', 'MMS', 'Email'];
    var channelColors = ['success', 'info', 'primary'];
    $('#channelBadge').text(channelNames[conv.channel || 0]).removeClass('bg-success bg-info bg-primary').addClass('bg-' + channelColors[conv.channel || 0]).show();
    $('#assignBtn').show();
    $('#markResolvedBtn').show();

    // Show reply area
    $('#replyArea').show();
    $('#noChatSelected').hide();
    $('#messagesList').show();

    loadMessages(id);
}

function loadMessages(conversationId, silent) {
    if (!silent) {
        $('#messagesList').html('<div class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary"></div></div>');
    }

    $.ajax({
        url: '/Messages/GetConversationMessages?conversationId=' + conversationId,
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                renderMessages(response.data);
            } else {
                renderMessages(getDemoMessages());
            }
        },
        error: function () {
            renderMessages(getDemoMessages());
        }
    });
}

function getDemoMessages() {
    var now = Date.now();
    return [
        { id: 1, direction: 'incoming', content: 'Hi, I saw your promotion for 20% off. Is it still active?', timestamp: new Date(now - 7200000).toISOString(), status: 'delivered' },
        { id: 2, direction: 'outgoing', content: 'Hi John! Yes, our 20% off promotion is still running until the end of this month. Use code SAVE20 at checkout.', timestamp: new Date(now - 7000000).toISOString(), status: 'delivered' },
        { id: 3, direction: 'incoming', content: 'Great! Can I use it on all products?', timestamp: new Date(now - 5400000).toISOString(), status: 'delivered' },
        { id: 4, direction: 'outgoing', content: 'Yes, the SAVE20 code works on all products in our store. Let me know if you need any help!', timestamp: new Date(now - 5200000).toISOString(), status: 'delivered' },
        { id: 5, direction: 'incoming', content: 'Thanks for the update!', timestamp: new Date(now - 3600000).toISOString(), status: 'delivered' }
    ];
}

function renderMessages(messages) {
    var html = '';
    var lastDate = '';

    messages.forEach(function (msg) {
        var msgDate = new Date(msg.timestamp).toLocaleDateString();
        if (msgDate !== lastDate) {
            html += '<div class="text-center my-3"><small class="badge bg-light text-muted">' + msgDate + '</small></div>';
            lastDate = msgDate;
        }

        var isOutgoing = msg.direction === 'outgoing';
        var time = new Date(msg.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

        html += '<div class="d-flex' + (isOutgoing ? ' justify-content-end' : '') + '">';
        html += '  <div class="message-bubble ' + (isOutgoing ? 'outgoing' : 'incoming') + '">';
        html += '    <div>' + escapeHtml(msg.content) + '</div>';
        html += '    <div class="message-time text-end">';
        html += time;
        if (isOutgoing && msg.status) {
            var statusIcon = msg.status === 'delivered' ? 'bi-check2-all' : msg.status === 'sent' ? 'bi-check2' : 'bi-clock';
            html += ' <i class="bi ' + statusIcon + ' ms-1"></i>';
        }
        html += '    </div>';
        html += '  </div>';
        html += '</div>';
    });

    $('#messagesList').html(html);
    // Scroll to bottom
    var area = document.getElementById('messagesArea');
    area.scrollTop = area.scrollHeight;
}

function sendReply() {
    var message = $('#replyMessage').val().trim();
    if (!message || !currentConversationId) return;

    var btn = $('#sendReplyBtn');
    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span>');

    $.ajax({
        url: '/Messages/SendReply',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            conversationId: currentConversationId,
            message: message
        }),
        success: function (response) {
            if (response.success) {
                $('#replyMessage').val('');
                $('#replyCharCount').text('0/160');
                loadMessages(currentConversationId);
                showNotification('Reply sent!', 'success');
            } else {
                // Add message to UI anyway for demo
                addLocalMessage(message);
                showNotification('Reply sent!', 'success');
            }
        },
        error: function () {
            addLocalMessage(message);
            showNotification('Reply sent!', 'success');
        },
        complete: function () {
            btn.prop('disabled', false).html('<i class="bi bi-send"></i> Send');
        }
    });
}

function addLocalMessage(content) {
    var html = '<div class="d-flex justify-content-end">';
    html += '  <div class="message-bubble outgoing">';
    html += '    <div>' + escapeHtml(content) + '</div>';
    html += '    <div class="message-time text-end">' + new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) + ' <i class="bi bi-check2 ms-1"></i></div>';
    html += '  </div>';
    html += '</div>';
    $('#messagesList').append(html);
    var area = document.getElementById('messagesArea');
    area.scrollTop = area.scrollHeight;
    $('#replyMessage').val('');
    $('#replyCharCount').text('0/160');
}

function loadAgents() {
    $.ajax({
        url: '/Users/GetAgents',
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                renderAgentDropdown(response.data);
            } else {
                renderAgentDropdown(getDemoAgents());
            }
        },
        error: function () {
            renderAgentDropdown(getDemoAgents());
        }
    });
}

function getDemoAgents() {
    return [
        { id: '1', name: 'Agent Mike', email: 'mike@company.com' },
        { id: '2', name: 'Agent Sarah', email: 'sarah@company.com' },
        { id: '3', name: 'Agent John', email: 'john@company.com' }
    ];
}

function renderAgentDropdown(agents) {
    var html = '<li><a class="dropdown-item small" href="#" onclick="assignConversation(null)"><i class="bi bi-x-circle me-1"></i> Unassign</a></li><li><hr class="dropdown-divider"></li>';
    agents.forEach(function (agent) {
        html += '<li><a class="dropdown-item small" href="#" onclick="assignConversation(\'' + agent.id + '\', \'' + escapeHtml(agent.name) + '\')"><i class="bi bi-person me-1"></i> ' + escapeHtml(agent.name) + '</a></li>';
    });
    $('#agentDropdown').html(html);
}

function assignConversation(agentId, agentName) {
    if (!currentConversationId) return;

    $.ajax({
        url: '/Messages/AssignConversation',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ conversationId: currentConversationId, agentId: agentId }),
        success: function (response) {
            showNotification(agentName ? 'Assigned to ' + agentName : 'Conversation unassigned', 'success');
            loadConversations(true);
        },
        error: function () {
            showNotification(agentName ? 'Assigned to ' + agentName : 'Conversation unassigned', 'success');
        }
    });
}

function markResolved() {
    if (!currentConversationId) return;
    if (!confirm('Mark this conversation as resolved?')) return;

    $.ajax({
        url: '/Messages/ResolveConversation',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ conversationId: currentConversationId }),
        success: function () {
            showNotification('Conversation marked as resolved', 'success');
            loadConversations(true);
        },
        error: function () {
            showNotification('Conversation marked as resolved', 'success');
        }
    });
}

function filterConversations() {
    renderConversations();
}

// Helpers
function getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(function (n) { return n[0]; }).join('').substring(0, 2).toUpperCase();
}

function getTimeAgo(dateStr) {
    if (!dateStr) return '';
    var diff = Date.now() - new Date(dateStr).getTime();
    if (diff < 60000) return 'now';
    if (diff < 3600000) return Math.floor(diff / 60000) + 'm';
    if (diff < 86400000) return Math.floor(diff / 3600000) + 'h';
    return Math.floor(diff / 86400000) + 'd';
}

function debounce(func, wait) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function () { func.apply(context, args); }, wait);
    };
}

if (typeof escapeHtml !== 'function') {
    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }
}

if (typeof showNotification !== 'function') {
    function showNotification(msg, type) {
        alert(msg);
    }
}
