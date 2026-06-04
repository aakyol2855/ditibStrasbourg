import http.server
import json
import os
import sys

PORT = 8080
DIRECTORY = os.path.dirname(os.path.abspath(__file__))

# S'assurer que le répertoire de travail est bien celui du script
os.chdir(DIRECTORY)

# Initialisation du fichier adresses.json à partir de adresses.js s'il n'existe pas
js_path = os.path.join(DIRECTORY, 'adresses.js')
json_path = os.path.join(DIRECTORY, 'adresses.json')

def init_json_data():
    if not os.path.exists(json_path):
        if os.path.exists(js_path):
            try:
                with open(js_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                # Extraire le JSON entre le premier [ et le dernier ]
                start = content.find('[')
                end = content.rfind(']') + 1
                if start != -1 and end != -1:
                    json_str = content[start:end]
                    data = json.loads(json_str)
                    # Sauvegarder dans adresses.json
                    with open(json_path, 'w', encoding='utf-8') as f_json:
                        json.dump(data, f_json, ensure_ascii=False, indent=4)
                    print("Initialisation réussie de adresses.json à partir de adresses.js")
                    return
            except Exception as e:
                print(f"Erreur lors de l'initialisation depuis adresses.js: {e}", file=sys.stderr)
        
        # Si aucun fichier n'existe ou en cas d'erreur, créer une liste vide
        with open(json_path, 'w', encoding='utf-8') as f_json:
            json.dump([], f_json, ensure_ascii=False, indent=4)
        print("Création d'un fichier adresses.json vide.")

init_json_data()

class CustomHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):

    def end_headers(self):
        self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
        super().end_headers()

    def do_GET(self):
        if self.path == '/admin' or self.path == '/admin/':
            self.path = '/admin.html'
        elif self.path.startswith('/api/logo'):
            logo_file = None
            for ext in ['.png', '.svg', '.jpg', '.jpeg']:
                p = os.path.join(DIRECTORY, f"logo{ext}")
                if os.path.exists(p):
                    logo_file = p
                    break
            
            if logo_file:
                try:
                    self.send_response(200)
                    ext = os.path.splitext(logo_file)[1].lower()
                    mime = 'image/png'
                    if ext == '.svg': mime = 'image/svg+xml'
                    elif ext in ['.jpg', '.jpeg']: mime = 'image/jpeg'
                    self.send_header('Content-Type', mime)
                    self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
                    self.end_headers()
                    with open(logo_file, 'rb') as f_img:
                        self.wfile.write(f_img.read())
                except Exception as e:
                    self.send_response(500)
                    self.end_headers()
            else:
                # Renvoyer le logo SVG DITIB par défaut au lieu d'une erreur 404
                try:
                    default_logo_svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 120" width="140" height="84">
                        <ellipse cx="100" cy="60" rx="90" ry="52" fill="#c22026" />
                        <ellipse cx="100" cy="60" rx="84" ry="46" fill="none" stroke="white" stroke-width="1.2" />
                        <g stroke="white" stroke-width="5.5" stroke-linecap="round" stroke-linejoin="round" fill="none">
                            <path d="M 68,69 H 45 C 39,69 36,66 36,60 V 33 C 36,27 39,24 44,24 C 49,24 49,30 49,69" />
                            <path d="M 132,69 H 155 C 161,69 164,66 164,60 V 33 C 164,27 161,24 156,24 C 151,24 151,30 151,69" />
                            <path d="M 84,46 V 69" />
                            <path d="M 116,46 V 69" />
                            <path d="M 100,38 V 69" />
                            <path d="M 76,36 H 124" />
                        </g>
                        <rect x="81.25" y="37" width="5.5" height="5.5" fill="white" />
                        <rect x="113.25" y="37" width="5.5" height="5.5" fill="white" />
                        <line x1="32" y1="81" x2="168" y2="81" stroke="white" stroke-width="1.5" />
                        <text x="100" y="95" fill="white" font-family="sans-serif" font-size="10.5" font-weight="800" text-anchor="middle" letter-spacing="2.8">STRASBOURG</text>
                    </svg>"""
                    self.send_response(200)
                    self.send_header('Content-Type', 'image/svg+xml')
                    self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
                    self.end_headers()
                    self.wfile.write(default_logo_svg.encode('utf-8'))
                except Exception:
                    self.send_response(404)
                    self.end_headers()
            return
        elif self.path.startswith('/api/config'):
            config_path = os.path.join(DIRECTORY, 'config.json')
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
            self.end_headers()
            
            default_config = {
                "titleTr": "Derneklerimiz",
                "titleFr": "Nos Associations",
                "strasbourgSub": "Ditib Merkez Camii",
                "websiteUrl": "www.ditibstrasbourg.fr",
                "styles": {
                    "deptLabelFontSize": "1.4",
                    "deptLabelColor": "#7e1e3f",
                    "assocLabelFontSize": "10",
                    "assocLabelColor": "#000000",
                    "strasbourgMainFontSize": "12",
                    "strasbourgSubFontSize": "9",
                    "strasbourgMainColor": "#c22026",
                    "strasbourgSubColor": "#000000"
                },
                "departmentLabels": [
                    { "name": "Bar-le-Duc", "code": "55", "coords": [48.96, 5.28] },
                    { "name": "Metz", "code": "57", "coords": [49.02, 6.45] },
                    { "name": "Strasbourg", "code": "67", "coords": [48.69, 7.56] },
                    { "name": "Colmar", "code": "68", "coords": [47.88, 7.28] },
                    { "name": "Épinal", "code": "88", "coords": [48.18, 6.45] },
                    { "name": "Nancy", "code": "54", "coords": [48.55, 6.00] },
                    { "name": "Chaumont", "code": "52", "coords": [48.00, 5.15] },
                    { "name": "Vesoul", "code": "70", "coords": [47.63, 5.95] },
                    { "name": "Besançon", "code": "25", "coords": [47.16, 6.18] },
                    { "name": "Belfort", "code": "90", "coords": [47.53, 6.95] }
                ],
                "associationsOverrides": {}
            }
            
            if os.path.exists(config_path):
                try:
                    with open(config_path, 'r', encoding='utf-8') as f:
                        config_data = json.load(f)
                    
                    # Merge default keys if they do not exist
                    for key, val in default_config.items():
                        if key not in config_data:
                            config_data[key] = val
                        elif isinstance(val, dict):
                            for subkey, subval in val.items():
                                if subkey not in config_data[key]:
                                    config_data[key][subkey] = subval
                    
                    self.wfile.write(json.dumps(config_data, ensure_ascii=False).encode('utf-8'))
                except Exception:
                    self.wfile.write(json.dumps(default_config, ensure_ascii=False).encode('utf-8'))
            else:
                self.wfile.write(json.dumps(default_config, ensure_ascii=False).encode('utf-8'))
            return
        return super().do_GET()

    def do_POST(self):
        if self.path == '/api/upload-logo':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            try:
                import base64
                req_data = json.loads(post_data.decode('utf-8'))
                file_data = req_data.get('data')
                filename = req_data.get('filename', 'logo.png')
                
                ext = os.path.splitext(filename)[1].lower()
                if ext not in ['.png', '.jpg', '.jpeg', '.svg']:
                    raise ValueError("Format de fichier non supporté. Autorisé: png, jpg, jpeg, svg")
                
                # Delete any old logos with other extensions
                for ex in ['.png', '.jpg', '.jpeg', '.svg']:
                    old_path = os.path.join(DIRECTORY, f"logo{ex}")
                    if os.path.exists(old_path):
                        try:
                            os.remove(old_path)
                        except Exception:
                            pass
                
                target_name = f"logo{ext}"
                target_path = os.path.join(DIRECTORY, target_name)
                
                header, encoded = file_data.split(",", 1) if "," in file_data else ("", file_data)
                binary_data = base64.b64decode(encoded)
                with open(target_path, 'wb') as f_img:
                    f_img.write(binary_data)
                
                self.send_response(200)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({"success": True, "filename": target_name}).encode('utf-8'))
                print(f"Logo mis à jour avec succès : {target_name}")
            except Exception as e:
                self.send_response(500)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({"success": False, "error": str(e)}).encode('utf-8'))
                print(f"Erreur lors du téléversement du logo : {e}", file=sys.stderr)
            return
        elif self.path == '/api/save-config':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            try:
                config_data = json.loads(post_data.decode('utf-8'))
                config_path = os.path.join(DIRECTORY, 'config.json')
                with open(config_path, 'w', encoding='utf-8') as f:
                    json.dump(config_data, f, ensure_ascii=False, indent=4)
                
                self.send_response(200)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({"success": True}).encode('utf-8'))
                print("Configuration de l'affiche sauvegardée.")
            except Exception as e:
                self.send_response(500)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({"success": False, "error": str(e)}).encode('utf-8'))
                print(f"Erreur lors de la sauvegarde de la configuration : {e}", file=sys.stderr)
            return

        elif self.path == '/api/add-address':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            try:
                new_addr = json.loads(post_data.decode('utf-8'))
                
                # Charger les adresses existantes
                with open(json_path, 'r', encoding='utf-8') as f:
                    adresses = json.load(f)
                
                # Générer un identifiant unique basé sur un index incrémental
                existing_ids = []
                for a in adresses:
                    if 'id' in a and a['id'].startswith('addr-'):
                        try:
                            existing_ids.append(int(a['id'].split('-')[1]))
                        except ValueError:
                            pass
                next_id_num = (max(existing_ids) + 1) if existing_ids else len(adresses)
                new_addr['id'] = f"addr-{next_id_num}"
                
                # Ajouter la nouvelle adresse
                adresses.append(new_addr)
                
                # Sauvegarder dans adresses.json
                with open(json_path, 'w', encoding='utf-8') as f:
                    json.dump(adresses, f, ensure_ascii=False, indent=4)
                
                # Régénérer adresses.js pour compatibilité avec le chargement JS classique
                with open(js_path, 'w', encoding='utf-8') as f:
                    f.write("const ADRESSES_DATA = ")
                    json.dump(adresses, f, ensure_ascii=False, indent=4)
                    f.write(";\n")
                
                # Répondre avec succès
                self.send_response(200)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                response = {"success": True, "data": new_addr}
                self.wfile.write(json.dumps(response, ensure_ascii=False).encode('utf-8'))
                print(f"Association ajoutée avec succès : {new_addr['name']}")
                
            except Exception as e:
                self.send_response(500)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                response = {"success": False, "error": str(e)}
                self.wfile.write(json.dumps(response).encode('utf-8'))
                print(f"Erreur lors de l'ajout d'adresse : {e}", file=sys.stderr)

        elif self.path == '/api/edit-address':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            try:
                updated_addr = json.loads(post_data.decode('utf-8'))
                addr_id = updated_addr.get('id')
                
                # Charger les adresses existantes
                with open(json_path, 'r', encoding='utf-8') as f:
                    adresses = json.load(f)
                
                # Trouver l'adresse et la mettre à jour
                found = False
                for i, a in enumerate(adresses):
                    if a.get('id') == addr_id:
                        adresses[i] = updated_addr
                        found = True
                        break
                
                if not found:
                    self.send_response(404)
                    self.send_header('Content-Type', 'application/json')
                    self.end_headers()
                    self.wfile.write(json.dumps({"success": False, "error": "Adresse introuvable"}).encode('utf-8'))
                    return
                
                # Sauvegarder dans adresses.json
                with open(json_path, 'w', encoding='utf-8') as f:
                    json.dump(adresses, f, ensure_ascii=False, indent=4)
                
                # Régénérer adresses.js
                with open(js_path, 'w', encoding='utf-8') as f:
                    f.write("const ADRESSES_DATA = ")
                    json.dump(adresses, f, ensure_ascii=False, indent=4)
                    f.write(";\n")
                
                # Répondre avec succès
                self.send_response(200)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                response = {"success": True, "data": updated_addr}
                self.wfile.write(json.dumps(response, ensure_ascii=False).encode('utf-8'))
                print(f"Association modifiée avec succès : {updated_addr['name']}")
                
            except Exception as e:
                self.send_response(500)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                response = {"success": False, "error": str(e)}
                self.wfile.write(json.dumps(response).encode('utf-8'))
                print(f"Erreur lors de la modification d'adresse : {e}", file=sys.stderr)

        elif self.path == '/api/delete-address':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            try:
                req_data = json.loads(post_data.decode('utf-8'))
                addr_id = req_data.get('id')
                
                # Charger les adresses existantes
                with open(json_path, 'r', encoding='utf-8') as f:
                    adresses = json.load(f)
                
                # Filtrer pour supprimer l'adresse
                initial_count = len(adresses)
                adresses = [a for a in adresses if a.get('id') != addr_id]
                
                if len(adresses) < initial_count:
                    # Sauvegarder dans adresses.json
                    with open(json_path, 'w', encoding='utf-8') as f:
                        json.dump(adresses, f, ensure_ascii=False, indent=4)
                    
                    # Régénérer adresses.js
                    with open(js_path, 'w', encoding='utf-8') as f:
                        f.write("const ADRESSES_DATA = ")
                        json.dump(adresses, f, ensure_ascii=False, indent=4)
                        f.write(";\n")
                    
                    self.send_response(200)
                    self.send_header('Content-Type', 'application/json')
                    self.end_headers()
                    self.wfile.write(json.dumps({"success": True}).encode('utf-8'))
                    print(f"Association supprimée avec succès: {addr_id}")
                else:
                    self.send_response(404)
                    self.send_header('Content-Type', 'application/json')
                    self.end_headers()
                    self.wfile.write(json.dumps({"success": False, "error": "Adresse introuvable"}).encode('utf-8'))
            except Exception as e:
                self.send_response(500)
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                response = {"success": False, "error": str(e)}
                self.wfile.write(json.dumps(response).encode('utf-8'))
                print(f"Erreur lors de la suppression d'adresse : {e}", file=sys.stderr)
        else:
            self.send_response(404)
            self.end_headers()

if __name__ == '__main__':
    # Permettre la liaison à 0.0.0.0 pour écouter sur tout le réseau local
    server_address = ('0.0.0.0', PORT)
    httpd = http.server.HTTPServer(server_address, CustomHTTPRequestHandler)
    print(f"Serveur démarré sur http://localhost:{PORT}")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nArrêt du serveur.")
        httpd.server_close()
