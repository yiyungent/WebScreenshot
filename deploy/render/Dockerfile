# render Dockerfile

FROM yiyungent/webscreenshot:v2.0.3

# 处于 /app 目录下
ADD render-entrypoint.sh ./render-entrypoint.sh
RUN chmod +x ./render-entrypoint.sh

ENTRYPOINT ["/bin/sh", "./render-entrypoint.sh"]
