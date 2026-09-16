// Client-side interactions for University Lost & Found
document.addEventListener('DOMContentLoaded', function () {
    // Image Preview Handler for File Uploads
    const imageInput = document.getElementById('imageFileInput');
    const imagePreviewContainer = document.getElementById('imagePreviewContainer');
    const imagePreview = document.getElementById('imagePreview');

    if (imageInput && imagePreview && imagePreviewContainer) {
        imageInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    imagePreview.src = e.target.result;
                    imagePreviewContainer.classList.remove('d-none');
                };
                reader.readAsDataURL(file);
            }
        });

        // Revalidate pages restored from the browser cache so completed forms do not
        // reappear when moving backward or forward through the workflow.
        window.addEventListener('pageshow', function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        });
    }

    // Auto dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});
