# Heroku Dockerfile

FROM yiyungent/webscreenshot:latest

ADD heroku-entrypoint.sh /heroku-entrypoint.sh

RUN chmod +x /heroku-entrypoint.sh

ENTRYPOINT ["/bin/sh", "/heroku-entrypoint.sh"]