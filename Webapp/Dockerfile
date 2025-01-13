#Stage 1 for building
FROM node:20
WORKDIR /usr/src/app

COPY package.json .
COPY package-lock.json .
COPY .env .

RUN npm install --omit=dev --quiet --frozen-lockfile

RUN npm install @next/bundle-analyzer

COPY . .

RUN npm run build

EXPOSE 3000

CMD ["yarn","run","start"]
#CMD ["yarn","run","dev"]
