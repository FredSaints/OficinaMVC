$(document).ready(function () {
    let unavailableDates = [];

    function fetchUnavailableDays(year, month, callback) {

        $.getJSON(`/Appointment/GetUnavailableDays?year=${year}&month=${month + 1}`, function (data) {
            unavailableDates = data;
            if (callback) {
                callback();
            }
        });
    }

    const fp = flatpickr("#AppointmentDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        minDate: "today",
        time_24hr: true,
        minuteIncrement: 30,

        disable: [
            function (date) {
                if (date.getDay() === 0 || date.getDay() === 6) {
                    return true;
                }
                const dateString = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
                return unavailableDates.includes(dateString);
            }
        ],

        onMonthChange: function (selectedDates, dateStr, instance) {
            const currentMonth = instance.currentMonth;
            const currentYear = instance.currentYear;
            fetchUnavailableDays(currentYear, currentMonth, function () {
                instance.redraw();
            });
        },

        onOpen: function (selectedDates, dateStr, instance) {
            const currentMonth = instance.currentMonth;
            const currentYear = instance.currentYear;
            if (unavailableDates.length === 0) {
                fetchUnavailableDays(currentYear, currentMonth, function () {
                    instance.redraw();
                });
            }
        }
    });


    const clientSelect = $('#ClientId');
    const vehicleSelect = $('#VehicleId');
    const appointmentDateInput = $('#AppointmentDate');
    const mechanicSelect = $('#MechanicId');

    clientSelect.on('change', function () {
        const clientId = $(this).val();
        vehicleSelect.empty().append('<option value="">Select a vehicle...</option>').prop('disabled', true);
        if (clientId) {
            $.getJSON(`/Appointment/GetVehiclesByClient?clientId=${clientId}`, function (data) {
                if (data && data.length > 0) {
                    vehicleSelect.prop('disabled', false);
                    $.each(data, function (index, item) {
                        vehicleSelect.append($('<option>', { value: item.value, text: item.text }));
                    });
                } else {
                    vehicleSelect.empty().append('<option value="">No vehicles found for this client</option>');
                }
            });
        }
    });

    function fetchAvailableMechanics() {
        const appointmentDate = appointmentDateInput.val();
        const timeValidationSpan = $('#time-validation-message');

        timeValidationSpan.text('');
        mechanicSelect.empty().append('<option value="">Select a date and time first...</option>').prop('disabled', true);

        if (appointmentDate) {
            $.getJSON(`/Appointment/GetAvailableMechanics?appointmentDate=${appointmentDate}`, function (data) {
                mechanicSelect.empty();

                if (data && data.length > 0) {
                    mechanicSelect.prop('disabled', false);

                    $.each(data, function (index, item) {
                        mechanicSelect.append($('<option>', {
                            value: item.value,
                            text: item.text
                        }));
                    });
                } else {
                    mechanicSelect.append('<option value="">No mechanics available</option>');
                    timeValidationSpan.text('The selected time is outside of all available work schedules. Please choose a different time.');
                }
            }).fail(function () {
                console.error('Failed to load mechanics.');
                mechanicSelect.empty().append('<option value="">Error loading mechanics</option>');
            });
        }
    }
    appointmentDateInput.on('change', fetchAvailableMechanics);
});