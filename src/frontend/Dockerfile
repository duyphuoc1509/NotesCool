# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

# Add build arg for API URL
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Copy package files
COPY src/frontend/package*.json ./

# Install dependencies (if any)
RUN npm install

# Copy source code
COPY src/frontend/ ./

# Build the application
RUN npm run build

# Stage 2: Runtime (Nginx)
FROM nginx:alpine
# Copy custom nginx config
COPY src/frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
