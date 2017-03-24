var player;
var asset;
$(document).ready(function(){
	var audio = $("#audioData");
	//asset = AV.Asset.fromURL(audio.data("uri"));
	//asset.decodeToBufffer(function(b){
	//});
	player = AV.Player.fromURL(audio.data("uri"));
	player.play();
});