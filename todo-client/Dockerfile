FROM node:24-alpine AS build
WORKDIR /app

COPY todo-client/package.json todo-client/package-lock.json ./
RUN npm ci

COPY todo-client/ ./
ARG VITE_API_HOST=localhost
ARG VITE_API_PORT=5121
ARG VITE_API_BASE_URL=http://localhost:5121/api
ARG VITE_API_BATCH_SIZE=100
ENV VITE_API_HOST=$VITE_API_HOST
ENV VITE_API_PORT=$VITE_API_PORT
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_API_BATCH_SIZE=$VITE_API_BATCH_SIZE
RUN npm run build

FROM nginx:alpine
COPY todo-client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
