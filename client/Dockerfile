# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy dependency manifests first (layer-cache friendly)
COPY package.json package-lock.json ./

# Install with legacy-peer-deps (required by ngx-charts peer conflicts)
RUN npm ci --legacy-peer-deps

# Copy the rest of the source
COPY . .

# Produce a production bundle
RUN npm run build

# ── Stage 2: Serve ──────────────────────────────────────────────────────────
FROM nginx:alpine AS serve

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy our SPA nginx config
COPY nginx.conf /etc/nginx/conf.d/angular.conf

# Copy built Angular app from previous stage
COPY --from=build /app/dist/gqm-plus/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
