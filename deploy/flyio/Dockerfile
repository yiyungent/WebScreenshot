# fly.io Dockerfile

FROM yiyungent/webscreenshot:v2.0.2

ADD flyio-entrypoint.sh ./flyio-entrypoint.sh
RUN chmod +x ./flyio-entrypoint.sh

ENTRYPOINT ["/bin/sh", "./flyio-entrypoint.sh"]
