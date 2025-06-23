$(document).ready(function () {
    const brandSelect = $('#BrandId');
    const modelSelect = $('#CarModelId');

    brandSelect.on('change', function () {
        const brandId = $(this).val();

        modelSelect.empty();

        if (brandId && brandId !== "0") {
            $.ajax({
                url: `/Vehicle/GetCarModels?brandId=${brandId}`,
                type: 'GET',
                success: function (data) {
                    modelSelect.prop('disabled', false);

                    $.each(data, function (index, carModel) {
                        modelSelect.append(
                            $('<option>', {
                                value: carModel.value,
                                text: carModel.text
                            })
                        );
                    });

                    const currentModelId = modelSelect.data('current-model-id');
                    if (currentModelId) {
                        modelSelect.val(currentModelId);
                    }
                },
                error: function (error) {
                    console.error("Error fetching car models: ", error);
                    modelSelect.prop('disabled', true);
                    modelSelect.append('<option value="">Error loading models</option>');
                }
            });
        } else {
            modelSelect.prop('disabled', true);
            modelSelect.append('<option value="">Select a brand first</option>');
        }
    });

    if (brandSelect.val() && brandSelect.val() !== "0") {
        brandSelect.trigger('change');
    }
});