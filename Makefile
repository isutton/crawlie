all:
	sudo docker build --pull -t crawlie . 

serve:
	sudo docker run -it --rm crawlie:latest
