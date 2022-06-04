$(() => {
	var form = $("form");


	var token = form.data("token");

	form.on("submit", f => {
		f.preventDefault();
		$.ajax({
			url: `/api/auth/register/${token}`,
			method: "POST",
			data: form.serialize()
		}).done(res => {
			window.location.assign("/");
		});

	});
});