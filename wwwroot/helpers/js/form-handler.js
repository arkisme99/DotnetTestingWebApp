function handleFormSubmission({ formSelector, buttonSelector }) {
    const $form = $(formSelector);
    const form = $form[0];
    const $btn = $(buttonSelector);

    $btn.click(function (event) {
        // Reset semua pesan error sebelum validasi
        $form.find('.invalid-feedback').text('');

        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();

            // Tambahkan pesan ke tiap field yang invalid
            $form.find(':input').each(function () {
                if (!this.checkValidity()) {
                    const $inputWrapper = $(this).closest('.form-group, .mb-3, .form-floating');
                    const $feedback = $inputWrapper.find('.invalid-feedback');

                    if ($feedback.length) {
                        let customMessage = this.validationMessage;

                        // Pesan untuk field yang `required`
                        if (this.validity.valueMissing) {
                            customMessage = "Kolom ini wajib diisi.";
                        }

                        // Pesan untuk email yang tidak valid
                        if (this.validity.typeMismatch && this.type === 'email') {
                            customMessage = "Format email tidak valid.";
                        }

                        // Pesan untuk `minlength` dan `maxlength`
                        if (this.validity.tooShort) {
                            customMessage = `Minimal ${this.getAttribute('minlength')} karakter diperlukan.`;
                        } else if (this.validity.tooLong) {
                            customMessage = `Maksimal ${this.getAttribute('maxlength')} karakter diizinkan.`;
                        }

                        // Pesan untuk input file yang belum dipilih
                        if (this.type === 'file' && this.files.length === 0) {
                            customMessage = "Silakan pilih file untuk diunggah.";
                        }

                        // Tampilkan pesan kustom ke feedback
                        $feedback.text(customMessage);
                    }
                }
            });

            $form.addClass('was-validated');
            return;
        }

        event.preventDefault();

        $btn.html(`<i class="fa fa-spinner fa-spin"></i> Loading...`).prop('disabled', true);
        form.submit();
    });
}
