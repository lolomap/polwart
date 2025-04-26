FROM node:lts-alpine as build-stage

# make the 'app' folder the current working directory
WORKDIR /app

# copy both 'package.json' and 'package-lock.json' (if available)
# use slashes in copy because of error in `Github Action` build process
COPY package*.json ./ 

# copy project files and folders to the current working directory (i.e. 'app' folder)
COPY . ./

# install project dependencies
RUN npm install

# build the app
RUN npm run build

FROM nginx as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]