# Heroku Dockerfile

FROM yiyungent/WebScreenshot:latest

ADD heroku-entrypoint.sh /heroku-entrypoint.sh

RUN chmod +x /heroku-entrypoint.sh

ENTRYPOINT ["/bin/sh", "/heroku-entrypoint.sh"]