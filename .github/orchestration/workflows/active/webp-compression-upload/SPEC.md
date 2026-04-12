# SPEC

## Scope

Add server-side conversion of uploaded images to WebP in the current asset upload pipeline.

## Affected Areas

- backend asset storage service
- backend upload metadata handling

## Technical Approach

- Use a server-side image library to decode uploaded images and save them as WebP.
- Update stored filename/content type metadata to match the converted output.
- Keep file-serving behavior unchanged beyond serving WebP files.

## Assumptions

- Current MVP upload types are image-only.

## Risks

- Some source formats may need careful handling during conversion.
