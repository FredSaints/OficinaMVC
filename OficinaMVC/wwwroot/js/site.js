// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    if ($('#notification-bell').length) {

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("ReceiveNotification", function (message, url, icon) {
            const notificationList = $('#notification-list');
            const countBadge = $('#notification-count');

            if (notificationList.find('.text-muted').length > 0) {
                notificationList.empty();
            }

            const newNotification = `
                <li>
                    <a class="dropdown-item d-flex align-items-start" href="${url}">
                        <i class="bi ${icon} text-primary me-3 mt-1"></i>
                        <div>
                            <p class="mb-0 small">${message}</p>
                            <small class="text-muted">${new Date().toLocaleTimeString()}</small>
                        </div>
                    </a>
                </li>`;

            notificationList.prepend(newNotification);

            let currentCount = parseInt(countBadge.text()) || 0;
            currentCount++;
            countBadge.text(currentCount).show();
        });

        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected.");
            } catch (err) {
                console.log(err);
                setTimeout(start, 5000);
            }
        };

        connection.onclose(async () => {
            await start();
        });

        start();

        $('#notification-bell').on('click', function () {
            $('#notification-count').text('').hide();
        });
    }
});