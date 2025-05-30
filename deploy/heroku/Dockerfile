# Heroku Dockerfile

FROM yiyungent/webscreenshot:v2.0.3

# 处于 /app 目录下
ADD heroku-entrypoint.sh ./heroku-entrypoint.sh
RUN chmod +x ./heroku-entrypoint.sh

ENTRYPOINT ["/bin/sh", "./heroku-entrypoint.sh"]
