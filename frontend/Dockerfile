FROM node:20 AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build -- --configuration production

FROM nginx AS final
WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=build /app/dist/korp-projeto-nota-fiscal/browser/. .
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80