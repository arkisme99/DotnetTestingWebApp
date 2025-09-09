$(document).ready(function() {
	$('.show-password-toggle').on('click', function() {
		
		var target = $(this).data('target'); // Ambil target input dari atribut data-target
		var inputField = $(target);
		var inputType = inputField.attr('type');

		// Toggle input type antara 'password' dan 'text'
		inputField.attr('type', inputType === 'password' ? 'text' : 'password');
	});
});
