# Railway Dockerfile

FROM yiyungent/webscreenshot:v2.0.1

# 处于 /app 目录下
ADD railway-entrypoint.sh ./railway-entrypoint.sh
RUN chmod +x ./railway-entrypoint.sh

ENTRYPOINT ["/bin/sh", "./railway-entrypoint.sh"]
