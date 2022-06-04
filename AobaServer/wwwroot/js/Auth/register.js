$(() => {
	var form = $("form");


	var token = form.data("token");

	form.on("submit", () => {

		$.ajax({
			url: `/api/auth/register/${token}`,
			method: "POST",
			data: form.serialize()
		}).done(res => {
			window.location.assign("/");
		});

	});
});