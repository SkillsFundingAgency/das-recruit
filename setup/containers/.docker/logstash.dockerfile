FROM docker.elastic.co/logstash/logstash-oss:7.0.0

RUN rm -f /usr/share/logstash/pipeline/logstash.conf

ADD ./.docker/logstash/pipeline/ /usr/share/logstash/pipeline/

ADD ./.docker/logstash/config/ /usr/share/logstash/config/