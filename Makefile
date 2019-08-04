all:
	sudo docker build -f Dockerfile.Crawlie.Server --pull -t crawlie-server . 
	sudo docker build -f Dockerfile.Crawlie.Client.App --pull -t crawlie-client . 

serve:
	sudo docker run -it --rm crawlie-server:latest
