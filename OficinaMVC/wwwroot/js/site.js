$(document).ready(function () {
    if ($('#notification-bell').length) {
        const NOTIFICATION_KEY = 'userNotifications';
        const notificationList = $('#notification-list');
        const countBadge = $('#notification-count');

        // --- 1. RENDER EXISTING NOTIFICATIONS ON PAGE LOAD ---
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

        // --- 2. ADD A NEW NOTIFICATION AND SAVE IT ---
        function addNotification(message, url, icon) {
            const storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');

            // --- THIS IS THE IMPROVED LINE ---
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

        // --- 3. REMOVE A NOTIFICATION AND SAVE THE CHANGE ---
        function removeNotification(targetId) {
            let storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            storedNotifications = storedNotifications.filter(n => n.id !== targetId);
            sessionStorage.setItem(NOTIFICATION_KEY, JSON.stringify(storedNotifications));
            renderNotifications();
        }

        // --- 4. UPDATE THE BADGE ---
        function updateBadgeCount() {
            const storedNotifications = JSON.parse(sessionStorage.getItem(NOTIFICATION_KEY) || '[]');
            if (storedNotifications.length > 0) {
                countBadge.text(storedNotifications.length).removeClass('d-none').show();
            } else {
                countBadge.text('0').addClass('d-none');
            }
        }

        // --- EVENT HANDLERS ---
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("ReceiveNotification", addNotification);

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

        $('#notification-bell').on('click', function () {
            // For simplicity, we can just clear the badge visually. 
            // A more complex system might have a separate "read" state.
            countBadge.text('0').addClass('d-none');
        });

        // --- INITIALIZATION ---
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