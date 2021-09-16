$(() => {
	var form = $("form");

	form.on("submit", f => {
		f.preventDefault();

		$.ajax({
			url: "/api/auth/login",
			data: form.serialize(),
			enctype: "application/x-www-form-urlencoded",
			method:"POST"
		}).done(() => {
			window.location = form.data("return");
		}).fail(res => {

		});
	});
});