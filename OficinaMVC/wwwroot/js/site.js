/**
 * Notification system for the FredAuto web application.
 * Handles rendering, adding, and removing notifications, as well as updating the notification badge.
 * Integrates with SignalR for real-time updates.
 *
 * Dependencies: jQuery, SignalR
 */
$(document).ready(function () {
    if ($('#notification-bell').length) {
        /**
         * Key for storing notifications in sessionStorage.
         * @type {string}
         */
        const NOTIFICATION_KEY = 'userNotifications';
        /**
         * jQuery element for the notification list dropdown.
         * @type {JQuery<HTMLElement>}
         */
        const notificationList = $('#notification-list');
        /**
         * jQuery element for the notification count badge.
         * @type {JQuery<HTMLElement>}
         */
        const countBadge = $('#notification-count');

        /**
         * Renders the notifications from sessionStorage into the dropdown list.
         */
        function renderNotifications() {
            const storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            notificationList.empty(); // Clear list before rendering

            if (storedNotifications.length === 0) {
                const placeholder = `<li id="no-notification-message" class="dropdown-item text-center text-muted p-3">No new notifications</li>`;
                notificationList.append(placeholder);
            } else {
                storedNotifications.forEach(notif => {
                    const notificationHtml = `
                        <li class="notification-item" id="${notif.id}">
                            <a class="dropdown-item d-flex align-items-start" href="${notif.url || '#'}" data-target-id="${notif.id}">
                                <i class="bi ${notif.icon || 'bi-info-circle'} fs-4 me-3 mt-1"></i>
                                <div>
                                    <p class="mb-0 small">${notif.message}</p>
                                    <small class="text-muted">${notif.time}</small>
                                </div>
                            </a>
                        </li>
                        <li><hr class="dropdown-divider my-0"></li>`;
                    notificationList.append(notificationHtml);
                });
            }
            updateBadgeCount();
        }

        /**
         * Adds a new notification to sessionStorage and updates the UI.
         * @param {string} message - The notification message.
         * @param {string} url - The URL to navigate to when the notification is clicked.
         * @param {string} icon - The icon class for the notification.
         */
        function addNotification(message, url, icon) {
            const storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');

            // Generate a unique ID for the notification
            const uniqueId = `notif-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

            const newNotif = {
                id: uniqueId, // Use the new, more robust ID
                message: message,
                url: url,
                icon: icon,
                time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
            };

            storedNotifications.unshift(newNotif);
            sessionStorage.setItem(NOTIFICATION_KEY, JSON.stringify(storedNotifications));
            renderNotifications();
        }

        /**
         * Removes a notification by its ID and updates the UI.
         * @param {string} targetId - The ID of the notification to remove.
         */
        function removeNotification(targetId) {
            let storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            storedNotifications = storedNotifications.filter(n => n.id !== targetId);
            sessionStorage.setItem(NOTIFICATION_KEY, JSON.stringify(storedNotifications));
            renderNotifications();
        }

        /**
         * Updates the notification badge count based on the number of notifications.
         */
        function updateBadgeCount() {
            const storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            if (storedNotifications.length > 0) {
                countBadge.text(storedNotifications.length).removeClass('d-none').show();
            } else {
                countBadge.text('0').addClass('d-none');
            }
        }

        // --- EVENT HANDLERS ---

        /**
         * SignalR connection for receiving real-time notifications.
         */
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Listen for notifications from the server
        connection.on("ReceiveNotification", addNotification);

        // Handle notification click: remove from storage and navigate
        notificationList.on('click', 'li.notification-item a.dropdown-item', function (e) {
            e.preventDefault();
            const destinationUrl = $(this).attr('href');
            const targetId = $(this).data('target-id');

            // Remove the notification from storage first
            let storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            storedNotifications = storedNotifications.filter(n => n.id !== targetId);
            sessionStorage.setItem(NOTIFICATION_KEY, JSON.stringify(storedNotifications));

            // Then navigate
            window.location.href = destinationUrl;
        });

        // Handle notification bell click: clear the badge visually
        $('#notification-bell').on('click', function () {
            // For simplicity, we can just clear the badge visually. 
            // A more complex system might have a separate "read" state.
            countBadge.text('0').addClass('d-none');
        });

        // --- INITIALIZATION ---

        /**
         * Starts the SignalR connection and handles reconnection logic.
         */
        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected.");
            } catch (err) {
                console.error("SignalR Connection Failed: ", err);
                setTimeout(start, 5000);
            }
        }
        connection.onclose(start);

        // Initial render on page load and start connection
        renderNotifications();
        start();
    }
});