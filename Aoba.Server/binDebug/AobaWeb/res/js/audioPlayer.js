var player;
var isPlaying;
var playIcon;
$(document).ready(function(){
	var audio = $("#audioPlayer");
	$("#playButton").click(TogglePlayPause);
	playIcon = $("#playIcon");
	var seekBarFill = $("#seekBar #fill");
	var seekBar = $("#seekBar");
	var title = $("#playbackContainer #title");
	var artist = $("#playbackContainer #artist");
	var time = $("#playbackContainer #time");
	var art = $("#playbackContainer #art");
	var duration = 1;
	player = AV.Player.fromURL(audio.data("uri"));
	player.on('duration', function(d) {
		duration = d;
	});
	seekBar.on('click', function(e){
		e.preventDefault();
		var pos = e.pageX - seekBar.offset().left;
		var seekP = pos/seekBar.width();
		if(seekP > 1)
			seekP = 1;
		else if(seekP < 0)
			seekP = 0;
		if(player.buffered > 0)
			player.seek(seekP * duration);
	});
	player.on('metadata', function(m){
		title.text(m.title);
		artist.text(m.artist);
		if(m.coverArt)
			art.attr("src", m.coverArt.toBlobURL());
	});
	player.on('progress', function(p) {
		seekBarFill.css("width", ((p/duration)* 100).toString() + "%");
		var sec = p / 1000;
		var min = Math.floor(sec/60);
		sec = Math.floor(((sec/60) - min)*60);
		if(sec < 10)
			sec = "0" + sec;
		time.text(min+":"+sec);
	});
	TogglePlayPause();
	
});

function TogglePlayPause()
{
	if(!isPlaying)
	{
		playIcon.attr("src", "/res/img/PauseButton.png");
		player.play();
	}
	else
	{
		playIcon.attr("src", "/res/img/PlayButton.png");
		player.pause();		
	}

	isPlaying = !isPlaying;
	
}