var username;
var password;
var sumbit;

$(document).ready(new function(e){
	username = $("#login input[name=username]");
	password = $("#login input[name=password]");
	sumbit = $("#login input[type=sumbit]").click(new function(e){
		e.preventDefault();
	});
});