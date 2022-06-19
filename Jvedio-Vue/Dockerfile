FROM nginx:1.21.1

COPY dist /usr/share/nginx/html
COPY k8s/nginx/nginx.conf /etc/nginx/nginx.conf
COPY k8s/nginx/conf.d/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80