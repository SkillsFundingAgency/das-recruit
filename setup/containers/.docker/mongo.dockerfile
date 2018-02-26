FROM mongo:latest

LABEL author "Lee Wadhams"

RUN apt-get update && apt-get install -y netcat-traditional netcat-openbsd

COPY ./.docker/mongo-scripts /mongo-scripts

RUN touch /.runonce

RUN chmod +rx /mongo-scripts/*.sh

EXPOSE 27017

CMD ["/mongo-scripts/start.sh"]