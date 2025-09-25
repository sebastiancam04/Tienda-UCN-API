```mermaid
sequenceDiagram
    participant C as Client
    participant API as API Server
    participant UR as User Repository
    participant VCR as VerificationCode Repository
    participant RS as Resend API

    Note over C,RS: Resend Verification Code

    C->>+API: POST /api/auth/resend-verification
    Note over C,API: Content-Type: application/json<br/>{<br/>"email": "user@example.com"<br/>}

    API->>+UR: _userRepository.GetByEmailAsync(body.Email)
    UR-->>-API: User user

    alt User not found
        Note over API: if (user == null)
        API-->>C: 400 Bad Request<br/>{"message": "Usuario no encontrado", "data": null}
    end

    alt Email already verified
        Note over API: if (user.EmailConfirmed == true)
        API-->>C: 400 Bad Request<br/>{"message": "El email ya está verificado", "data": null}
    end

    Note over API: var codeType = CodeType.VerifyEmailCode

    API->>+VCR: _verificationCodeRepository.GetLatestByUserAndTypeAsync(user.Id, codeType)
    VCR-->>-API: VerificationCode lastCode

    alt Rate limited
        Note over API: if (lastCode.CreatedAt.AddSeconds(SECONDS_TO_EXPIRY) > DateTime.UtcNow)
        Note over API: var remainingSeconds = SECONDS_TO_EXPIRY - (DateTime.UtcNow - lastCode.CreatedAt).TotalSeconds

        API-->>C: 429 Too Many Requests<br/>Retry-After: {remainingSeconds}<br/>{"message": "Debes esperar antes de solicitar otro código", "data": null}
    end

    Note over API: var newCode = Random.Next(100000, 999999)<br/>var newExpiryDate = DateTime.UtcNow.AddSeconds(SECONDS_TO_EXPIRY)

    API->>+VCR: _verificationCodeRepository.UpdateAsync(lastCode.Id, newCode, newExpiryDate)
    VCR-->>-API: VerificationCode updatedCode

    API->>+RS: POST https://api.resend.com/emails
    Note over API,RS: New Verification Email<br/>Authorization: Bearer re_api_key<br/>{<br/>"from": "<onboarding@resend.dev>",<br/>"to": ["user@example.com"],<br/>"subject": "Nuevo código de verificación - Tienda UCN",<br/>"html": "Template con nuevo código: {newCode}"<br/>}

    RS-->>-API: 200 OK<br/>{"id": "email_id_456"}

    API-->>-C: 200 OK<br/>{"message": "Código de verificación reenviado a tu email", "data": null}
```
