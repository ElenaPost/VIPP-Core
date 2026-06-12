const connection = new signalR.HubConnectionBuilder()
    .withUrl("/feedbackHub")
    .build();

connection.start().catch(err => console.error(err));
